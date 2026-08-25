import logging
import os
import aiohttp
import asyncio
import json
from livekit.agents import (
    AutoSubscribe,
    JobContext,
    WorkerOptions,
    cli,
    Agent,
    llm,
    AgentSession
)
from livekit.plugins import google

logger = logging.getLogger("ai-worker")
logger.setLevel(logging.INFO)

async def fetch_persona(http_session, persona_id, backend_url):
    """Fetch persona instructions and config from backend API."""
    try:
        url = f"{backend_url}/api/personas/{persona_id}/published"
        async with http_session.get(url) as resp:
            if resp.status == 200:
                data = await resp.json()
                logger.info(f"Loaded persona: {data.get('personaName', persona_id)}")
                return data.get("systemPrompt"), data.get("configurationJson", "{}")
            else:
                logger.warning(f"Failed to fetch persona (status {resp.status}), using defaults")
    except Exception as e:
        logger.warning(f"Failed to fetch persona: {e}, using defaults")
    return None, None

async def entrypoint(ctx: JobContext):
    logger.info(f"Starting LiveKit AgentSession for room: {ctx.room.name}")

    google_api_key = os.environ.get("GOOGLE_API_KEY") or os.environ.get("GEMINI_API_KEY")
    if not google_api_key:
        raise ValueError("GOOGLE_API_KEY or GEMINI_API_KEY environment variable not set")
    
    os.environ["GOOGLE_API_KEY"] = google_api_key

    backend_url = os.environ.get("BACKEND_URL", "http://backend:5000")
    persona_id = os.environ.get("PERSONA_ID", None)
    voice = os.environ.get("AGENT_VOICE", "Aoede")
    model_name = os.environ.get("AGENT_MODEL", "models/gemini-3.1-flash-live-preview")
    temperature = float(os.environ.get("AGENT_TEMPERATURE", "0.7"))

    instructions = """أنتِ موظفة كول سنتر في مطعم سوري. 
تحدثي باللهجة المصرية بأسلوب ودود.
وظيفتك استقبال الطلبات والمساعدة.
إذا طلب العميل التحدث مع موظف بشري (خدمة العملاء الحقيقية)، لا تقولي 'سأقوم بتحويلك'، بل استخدمي أداة transfer_to_human فوراً في صمت. بناءً على نتيجة الأداة، أخبري العميل إما أنه جاري التحويل أو أن خدمة العملاء غير متاحة."""

    config_json = "{}"

    if persona_id:
        async with aiohttp.ClientSession() as session:
            persona_instructions, persona_config = await fetch_persona(session, persona_id, backend_url)
            if persona_instructions:
                instructions = persona_instructions
            if persona_config:
                config_json = persona_config
                try:
                    cfg = json.loads(config_json)
                    if cfg.get("voice"):
                        voice = cfg["voice"]
                    if cfg.get("model"):
                        model_name = cfg["model"]
                    if cfg.get("temperature") is not None:
                        temperature = float(cfg["temperature"])
                except Exception:
                    pass

    logger.info(f"Using model={model_name}, voice={voice}, temp={temperature}")

    model = google.beta.realtime.RealtimeModel(
        voice=voice,
        temperature=temperature,
        model=model_name
    )

    transfer_in_progress = False
    last_transfer_reason = ""

    @llm.function_tool(description="تحويل المكالمة إلى موظف بشري عند طلب العميل")
    async def transfer_to_human(reason: str = ""):
        nonlocal transfer_in_progress, last_transfer_reason
        last_transfer_reason = reason
        logger.info(f"Transferring call in room {ctx.room.name} to human. Reason: {reason}")
        async with aiohttp.ClientSession() as http_session:
            try:
                async with http_session.post(
                    f"{backend_url}/api/call/transfer",
                    json={"RoomName": ctx.room.name}
                ) as resp:
                    if resp.status == 200:
                        transfer_in_progress = True
                        return "جاري الاتصال بالموظف، يرجى البقاء معي على الخط لحظات..."
                    elif resp.status == 400:
                        return "عفواً، لا يوجد موظفين متاحين الآن، هل يمكنني مساعدتك في شيء آخر؟"
                    else:
                        body = await resp.text()
                        logger.warning(f"Transfer returned {resp.status}: {body}")
                        return "عفواً، لا يوجد موظفين متاحين الآن، هل يمكنني مساعدتك في شيء آخر؟"
            except Exception as e:
                logger.error(f"Failed to transfer: {e}")
                return "عذراً، حدث خطأ أثناء التحويل."

    agent = Agent(instructions=instructions)
    session = AgentSession(llm=model, tools=[transfer_to_human])
    
    @ctx.room.on("participant_connected")
    def on_participant_connected(participant):
        nonlocal transfer_in_progress, last_transfer_reason
        logger.info(f"Participant {participant.identity} connected.")
        if transfer_in_progress and participant.identity.startswith("agent_"):
            logger.info("Human agent joined! AI is leaving the room gracefully.")
            
            async def leave_now():
                await asyncio.sleep(0.5)

                summary_text = (
                    f"Customer requested transfer. Reason given: {last_transfer_reason or 'Customer requested human assistance'}"
                )

                async with aiohttp.ClientSession() as http_session:
                    try:
                        await http_session.post(
                            f"{backend_url}/api/call/summary",
                            json={
                                "RoomName": ctx.room.name,
                                "Summary": summary_text
                            }
                        )
                        logger.info("Handoff summary sent to backend")
                    except Exception as e:
                        logger.warning(f"Failed to send handoff summary: {e}")

                await ctx.room.disconnect()
            
            asyncio.create_task(leave_now())

    @ctx.room.on("participant_disconnected")
    def on_participant_disconnected(participant):
        logger.info(f"Participant {participant.identity} left, marking call as ended.")
        async def mark_ended():
            async with aiohttp.ClientSession() as http_session:
                try:
                    await http_session.post(
                        f"{backend_url}/api/call/end",
                        json={"RoomName": ctx.room.name}
                    )
                except Exception as e:
                    logger.warning(f"Failed to end call: {e}")
        asyncio.create_task(mark_ended())

    await session.start(agent, room=ctx.room)
    await ctx.connect(auto_subscribe=AutoSubscribe.AUDIO_ONLY)

    async with aiohttp.ClientSession() as http_session:
        try:
            await http_session.post(
                f"{backend_url}/api/call/active",
                json={"RoomName": ctx.room.name}
            )
        except Exception as e:
            logger.warning(f"Failed to register active call: {e}")
    
    try:
        await session.generate_reply(
            instructions="رحبي بالعميل وعرّفي عن نفسك كموظفة."
        )
    except Exception as exc:
        logger.warning("Initial greeting failed: %s", exc)

if __name__ == "__main__":
    cli.run_app(WorkerOptions(entrypoint_fnc=entrypoint))
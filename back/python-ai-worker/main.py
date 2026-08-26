import asyncio
import json
import logging
import os

import aiohttp
from livekit.agents import (
    AutoSubscribe,
    JobContext,
    JobRequest,
    WorkerOptions,
    cli,
    Agent,
    llm,
    AgentSession,
)
from livekit.plugins import google

logger = logging.getLogger("ai-worker")
logger.setLevel(logging.INFO)

AI_IDENTITY = "ai-agent"


async def fetch_persona(http_session, persona_id, backend_url):
    """Fetch persona instructions and config from backend API."""
    headers = {}
    token = os.environ.get("BACKEND_SERVICE_TOKEN")
    if token:
        headers["X-Service-Token"] = token
    try:
        url = f"{backend_url}/api/personas/{persona_id}/published"
        async with http_session.get(url, headers=headers) as resp:
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
    voice = os.environ.get("AGENT_VOICE", "Aoede")
    model_name = os.environ.get("AGENT_MODEL", "models/gemini-3.1-flash-live-preview")
    temperature = float(os.environ.get("AGENT_TEMPERATURE", "0.7"))

    # Routing metadata from the backend's CreateAgentDispatch call.
    persona_id = os.environ.get("PERSONA_ID") or None
    session_id = None
    try:
        meta = json.loads(ctx.job.metadata or "{}")
        if isinstance(meta, dict):
            persona_id = meta.get("personaId") or persona_id
            session_id = meta.get("sessionId")
    except Exception:
        pass

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

    logger.info(f"Using model={model_name}, voice={voice}, temp={temperature}, "
                f"persona={persona_id}, session={session_id}")

    model = google.beta.realtime.RealtimeModel(
        voice=voice,
        temperature=temperature,
        model=model_name
    )

    transfer_in_progress = False
    last_transfer_reason = ""

    def _service_headers():
        headers = {}
        token = os.environ.get("BACKEND_SERVICE_TOKEN")
        if token:
            headers["X-Service-Token"] = token
        return headers

    @llm.function_tool(description="تحويل المكالمة إلى موظف بشري عند طلب العميل. مرري اسم الموظف إذا حدده العميل.")
    async def transfer_to_human(name: str = "", reason: str = ""):
        nonlocal transfer_in_progress, last_transfer_reason
        last_transfer_reason = reason
        logger.info(f"Transferring call in room {ctx.room.name} to human '{name}'. Reason: {reason}")
        async with aiohttp.ClientSession() as http_session:
            try:
                async with http_session.post(
                    f"{backend_url}/api/call/transfer",
                    headers=_service_headers(),
                    json={
                        "RoomName": ctx.room.name,
                        "TargetType": "human",
                        "TargetName": name or None,
                        "Reason": reason or None,
                    },
                ) as resp:
                    if resp.status == 200:
                        body = await resp.json()
                        agent_name = body.get("agentName") or ""
                        transfer_in_progress = True
                        return f"جاري الاتصال بالموظف {agent_name}، يرجى البقاء معي على الخط لحظات..."
                    elif resp.status == 400:
                        body = await resp.json()
                        return f"عفواً، {body.get('error', 'لا يوجد موظفين متاحين الآن')}، هل يمكنني مساعدتك في شيء آخر؟"
                    else:
                        body = await resp.text()
                        logger.warning(f"Transfer returned {resp.status}: {body}")
                        return "عفواً، لا يوجد موظفين متاحين الآن، هل يمكنني مساعدتك في شيء آخر؟"
            except Exception as e:
                logger.error(f"Failed to transfer: {e}")
                return "عذراً، حدث خطأ أثناء التحويل."

    @llm.function_tool(description="تحويل المكالمة إلى قسم خارجي حسب الاسم مثل: دعم، مبيعات، فواتير.")
    async def transfer_to_department(name: str, reason: str = ""):
        nonlocal transfer_in_progress, last_transfer_reason
        last_transfer_reason = reason
        logger.info(f"Transferring call in room {ctx.room.name} to destination '{name}'. Reason: {reason}")
        if not name:
            return "عفواً، لم أحدد القسم المطلوب. هل يمكنك تسمية القسم؟"
        async with aiohttp.ClientSession() as http_session:
            try:
                async with http_session.post(
                    f"{backend_url}/api/call/transfer",
                    headers=_service_headers(),
                    json={
                        "RoomName": ctx.room.name,
                        "TargetType": "destination",
                        "TargetName": name,
                        "Reason": reason or None,
                    },
                ) as resp:
                    if resp.status == 200:
                        transfer_in_progress = True
                        return f"جاري تحويلك إلى قسم {name}، يرجى البقاء على الخط لحظات..."
                    elif resp.status == 400:
                        body = await resp.json()
                        return f"عفواً، {body.get('error', 'هذا القسم غير متاح حالياً')}. كيف يمكنني مساعدتك؟"
                    else:
                        body = await resp.text()
                        logger.warning(f"Destination transfer returned {resp.status}: {body}")
                        return "عفواً، هذا القسم غير متاح حالياً. كيف يمكنني مساعدتك؟"
            except Exception as e:
                logger.error(f"Failed to transfer to destination: {e}")
                return "عذراً، حدث خطأ أثناء التحويل."

    @llm.function_tool(description="ابحثي في قاعدة المعرفة للعثور على معلومات دقيقة عن المنتجات أو الأسعار أو السياسات قبل الإجابة. استخدميها كلما احتجت معلومة لست متأكدة منها.")
    async def search_knowledge(query: str):
        if not persona_id or not query:
            return "لا توجد معلومات متاحة."
        try:
            from urllib.parse import quote
            async with aiohttp.ClientSession() as http_session:
                async with http_session.get(
                    f"{backend_url}/api/personas/{persona_id}/knowledge-context",
                    headers=_service_headers(),
                    params={"query": query, "topK": "4"},
                ) as resp:
                    if resp.status != 200:
                        return "لا توجد معلومات متاحة في قاعدة المعرفة."
                    items = await resp.json()
                    if not items:
                        return "لا توجد معلومات متاحة في قاعدة المعرفة بهذا الشأن."
                    lines = [
                        f"- {item.get('content','').strip()}"
                        + (f" (المصدر: {item['documentName']})" if item.get('documentName') else "")
                        for item in items
                    ]
                    return "نتائج من قاعدة المعرفة:\n" + "\n".join(lines)
        except Exception as e:
            logger.warning(f"Knowledge search failed: {e}")
            return "تعذر البحث في قاعدة المعرفة حالياً."

    agent = Agent(instructions=instructions)
    session = AgentSession(llm=model, tools=[transfer_to_human, transfer_to_department, search_knowledge])

    @ctx.room.on("participant_connected")
    def on_participant_connected(participant):
        nonlocal transfer_in_progress, last_transfer_reason
        logger.info(f"Participant {participant.identity} connected.")
        is_swapped_in = (
            transfer_in_progress
            and (
                participant.identity.startswith("agent_")
                or participant.identity.startswith("dest_")
            )
        )
        if is_swapped_in:
            logger.info(f"Swap target {participant.identity} joined! AI is leaving the room.")

            async def leave_now():
                await asyncio.sleep(0.5)

                summary_text = (
                    f"Customer requested transfer. Reason given: {last_transfer_reason or 'Customer requested human assistance'}"
                )

                async with aiohttp.ClientSession() as http_session:
                    try:
                        await http_session.post(
                            f"{backend_url}/api/call/summary",
                            headers=_service_headers(),
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
        if participant.identity == AI_IDENTITY:
            return  # our own removal after a swap must not end the call
        logger.info(f"Participant {participant.identity} left, marking call as ended.")
        async def mark_ended():
            async with aiohttp.ClientSession() as http_session:
                try:
                    await http_session.post(
                        f"{backend_url}/api/call/end",
                        headers=_service_headers(),
                        json={"RoomName": ctx.room.name}
                    )
                except Exception as e:
                    logger.warning(f"Failed to end call: {e}")

                # Metering: one idempotent call_minutes row per AI-handled call.
                if session_id and connected_at:
                    minutes = round((asyncio.get_event_loop().time() - connected_at) / 60, 2)
                    if minutes > 0:
                        try:
                            await http_session.post(
                                f"{backend_url}/api/usage",
                                headers=_service_headers(),
                                params={
                                    "metricType": "call_minutes",
                                    "quantity": str(minutes),
                                    "unit": "minutes",
                                    "callSessionId": session_id,
                                },
                            )
                            logger.info(f"Recorded {minutes} call_minutes for session {session_id}")
                        except Exception as e:
                            logger.warning(f"Failed to record usage: {e}")
        asyncio.create_task(mark_ended())

    connected_at = None
    await session.start(agent, room=ctx.room)
    # Pinned livekit-agents 1.6.10: connect() has no identity kwarg — the
    # participant identity is fixed by job acceptance (see request_handler).
    await ctx.connect(auto_subscribe=AutoSubscribe.AUDIO_ONLY)
    connected_at = asyncio.get_event_loop().time()

    logger.info(f"Joined room {ctx.room.name} as {AI_IDENTITY} "
                f"(session={session_id}, persona={persona_id})")

    async with aiohttp.ClientSession() as http_session:
        try:
            await http_session.post(
                f"{backend_url}/api/call/active",
                headers=_service_headers(),
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

async def request_handler(job_request: JobRequest):
    """Pin the agent participant identity so backend webhook routing and the
    cold-swap RemoveParticipant can rely on it."""
    await job_request.accept(identity=AI_IDENTITY)


if __name__ == "__main__":
    cli.run_app(WorkerOptions(
        entrypoint_fnc=entrypoint,
        request_fnc=request_handler,
        agent_name=os.environ.get("AGENT_NAME", "voice-agent"),
    ))

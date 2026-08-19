import logging
import os
import aiohttp
import asyncio
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

async def entrypoint(ctx: JobContext):
    logger.info("Starting LiveKit AgentSession for Gemini Realtime...")

    google_api_key = os.environ.get("GOOGLE_API_KEY") or os.environ.get("GEMINI_API_KEY")
    if not google_api_key:
        raise ValueError("GOOGLE_API_KEY or GEMINI_API_KEY environment variable not set")
    
    os.environ["GOOGLE_API_KEY"] = google_api_key

    model = google.beta.realtime.RealtimeModel(voice="Aoede")
    
    instructions = """أنتِ موظفة كول سنتر في مطعم سوري. 
تحدثي باللهجة المصرية بأسلوب ودود.
وظيفتك استقبال الطلبات والمساعدة.
إذا طلب العميل التحدث مع موظف بشري (خدمة العملاء الحقيقية)، استخدمي أداة transfer_to_human فوراً وقولي له أنه سيتم تحويله."""

    @llm.function_tool(description="تحويل المكالمة إلى موظف بشري عند طلب العميل")
    async def transfer_to_human(reason: str = ""):
        logger.info(f"Transferring call in room {ctx.room.name} to human. Reason: {reason}")
        async with aiohttp.ClientSession() as http_session:
            try:
                async with http_session.post("http://backend:5000/api/call/transfer", json={"RoomName": ctx.room.name}) as resp:
                    if resp.status == 200:
                        asyncio.create_task(leave_room_soon(ctx))
                        return "جاري تحويلك للموظف، ثواني معدودة"
                    else:
                        return "عفواً، لا يوجد موظفين متاحين الآن، هل يمكنني مساعدتك في شيء آخر؟"
            except Exception as e:
                logger.error(f"Failed to transfer: {e}")
                return "عذراً، حدث خطأ أثناء التحويل."

    async def leave_room_soon(ctx_local):
        await asyncio.sleep(5)
        await ctx_local.room.disconnect()

    # Pass tools to AgentSession directly, and use the regular Agent
    agent = Agent(instructions=instructions)
    session = AgentSession(llm=model, tools=[transfer_to_human])
    
    @ctx.room.on("participant_disconnected")
    def on_participant_disconnected(participant):
        logger.info(f"Participant {participant.identity} left, marking call as ended.")
        async def mark_ended():
            async with aiohttp.ClientSession() as http_session:
                try:
                    await http_session.post("http://backend:5000/api/call/end", json={"RoomName": ctx.room.name})
                except Exception as e:
                    logger.warning(f"Failed to end call: {e}")
        asyncio.create_task(mark_ended())

    await session.start(agent, room=ctx.room)
    await ctx.connect(auto_subscribe=AutoSubscribe.AUDIO_ONLY)

    # Register the call as active in the backend DB (especially useful for SIP calls)
    async with aiohttp.ClientSession() as http_session:
        try:
            await http_session.post("http://backend:5000/api/call/active", json={"RoomName": ctx.room.name})
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

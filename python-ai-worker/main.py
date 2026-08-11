import logging
import os
from livekit.agents import (
    AutoSubscribe,
    JobContext,
    WorkerOptions,
    cli,
    Agent,
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
    
    instructions = """أنتِ موظفة كول سنتر (بنت) في مطعم سوري للأكل العربي.
يجب أن تتحدثي باللهجة المصرية العفوية، وتكوني متفاعلة، ودودة، ومرحبة جداً بالزبائن.
وظيفتك هي استقبال الطلبات، الإجابة على الاستفسارات حول المنيو (شاورما، كريسبي، بروستد، مقبلات سورية)، واقتراح وجبات.
تحدثي باختصار، وتفاعلي مع العميل بشكل طبيعي كأنك في مكالمة هاتفية حقيقية."""

    agent = Agent(instructions=instructions)
    session = AgentSession(llm=model)
    
    await session.start(agent, room=ctx.room)
    
    await ctx.connect(auto_subscribe=AutoSubscribe.AUDIO_ONLY)
    
    try:
        await session.generate_reply(
            instructions="رحبي بالعميل باللهجة المصرية وعرّفي عن نفسك كموظفة في المطعم السوري."
        )
    except Exception as exc:
        logger.warning("Initial greeting failed: %s", exc)

if __name__ == "__main__":
    cli.run_app(WorkerOptions(entrypoint_fnc=entrypoint))

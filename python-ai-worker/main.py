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

    await ctx.connect(auto_subscribe=AutoSubscribe.AUDIO_ONLY)

    # Note: gemini-2.0-flash-exp requires the Beta API. 
    model = google.beta.realtime.RealtimeModel()
    
    agent = Agent(instructions="You are a helpful call center assistant. Please answer concisely and politely in Arabic. Introduce yourself briefly.")
    session = AgentSession(llm=model)
    await session.start(agent, room=ctx.room)

if __name__ == "__main__":
    cli.run_app(WorkerOptions(entrypoint_fnc=entrypoint))

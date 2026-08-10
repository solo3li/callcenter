import asyncio
import logging
import os
from livekit.agents import (
    AutoSubscribe,
    JobContext,
    WorkerOptions,
    cli,
    Agent,
)
from livekit.plugins import google

logger = logging.getLogger("ai-worker")
logger.setLevel(logging.INFO)

async def entrypoint(ctx: JobContext):
    logger.info("Starting Gemini Multimodal AI Worker...")
    await ctx.connect(auto_subscribe=AutoSubscribe.AUDIO_ONLY)

    # Use Gemini Multimodal Live API
    agent = Agent(
        llm=google.beta.realtime.RealtimeModel(
            model="gemini-3.1-flash-live-preview",
            voice="Puck",
            instructions="You are a helpful call center assistant. Answer concisely and politely."
        ),
    )

    agent.start(ctx.room)
    
    await asyncio.sleep(1)

if __name__ == "__main__":
    cli.run_app(WorkerOptions(entrypoint_fnc=entrypoint, worker_type="room"))

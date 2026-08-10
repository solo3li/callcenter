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
    try:
        await ctx.connect()
    except Exception as e:
        logger.error(f"Error in connect: {e}", exc_info=True)
        raise

    try:
        # Use Gemini Multimodal Live API
        agent = Agent(
            instructions="You are a helpful call center assistant. Answer concisely and politely.",
            llm=google.beta.realtime.RealtimeModel(
                model="gemini-3.1-flash-live-preview",
                voice="Puck",
            ),
        )
        agent.start(ctx.room)
    except Exception as e:
        logger.error(f"Error in agent setup: {e}", exc_info=True)
        raise
    
    await asyncio.sleep(1)

if __name__ == "__main__":
    cli.run_app(WorkerOptions(entrypoint_fnc=entrypoint))

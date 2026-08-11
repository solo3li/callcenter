import logging
import os
from livekit.agents import (
    AutoSubscribe,
    JobContext,
    WorkerOptions,
    cli,
)
from livekit.agents.multimodal import MultimodalAgent
from livekit.plugins import google

logger = logging.getLogger("ai-worker")
logger.setLevel(logging.INFO)

async def entrypoint(ctx: JobContext):
    logger.info("Starting LiveKit MultimodalAgent for Gemini Realtime...")

    # Ensure the Google API key is set for the plugin
    google_api_key = os.environ.get("GOOGLE_API_KEY") or os.environ.get("GEMINI_API_KEY")
    if not google_api_key:
        raise ValueError("GOOGLE_API_KEY or GEMINI_API_KEY environment variable not set")
    
    # We must explicitly set GOOGLE_API_KEY for the google-genai SDK used internally by the plugin
    os.environ["GOOGLE_API_KEY"] = google_api_key

    # Connect to the room and auto-subscribe to incoming audio
    await ctx.connect(auto_subscribe=AutoSubscribe.AUDIO_ONLY)

    # Initialize the MultimodalAgent using the Gemini Realtime model
    agent = MultimodalAgent(
        model=google.beta.realtime.RealtimeModel(
            instructions="You are a helpful call center assistant. Please answer concisely and politely in Arabic. Introduce yourself briefly."
        )
    )

    # Start the agent and link it to the room
    agent.start(ctx.room)

    # Trigger the agent to speak an initial greeting
    # Note: MultimodalAgent will handle WebRTC streams, text-to-speech routing, etc. automatically.
    agent.generate_reply()


if __name__ == "__main__":
    cli.run_app(WorkerOptions(entrypoint_fnc=entrypoint))

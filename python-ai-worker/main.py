import asyncio
import logging
from livekit.agents import (
    AutoSubscribe,
    JobContext,
    WorkerOptions,
    cli,
    llm,
)
from livekit.agents.pipeline import VoicePipelineAgent
from livekit.plugins import google, silero

logger = logging.getLogger("ai-worker")
logger.setLevel(logging.INFO)

async def entrypoint(ctx: JobContext):
    logger.info("Starting AI Worker...")
    await ctx.connect(auto_subscribe=AutoSubscribe.AUDIO_ONLY)

    initial_ctx = llm.ChatContext().append(
        role="system",
        text=(
            "You are a helpful AI assistant connected to a phone call in a call center. "
            "Speak naturally, clearly, and concisely. Keep responses brief."
        ),
    )

    # Use VoicePipelineAgent which handles VAD, STT, LLM, TTS, and barge-in automatically
    agent = VoicePipelineAgent(
        vad=silero.VAD.load(),
        stt=google.STT(),
        llm=google.LLM(model="gemini-1.5-flash"), # Fallback to standard fast model
        tts=google.TTS(),
        chat_ctx=initial_ctx,
    )

    agent.start(ctx.room)
    
    await asyncio.sleep(1)
    await agent.say("مرحباً بك، أنا المساعد الذكي الخاص بمركز الاتصال. كيف يمكنني مساعدتك اليوم؟")

if __name__ == "__main__":
    cli.run_app(WorkerOptions(entrypoint_fnc=entrypoint, worker_type="room"))

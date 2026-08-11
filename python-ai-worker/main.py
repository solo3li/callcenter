import asyncio
import logging
import os
import traceback
from livekit import rtc
from livekit.agents import (
    JobContext,
    WorkerOptions,
    cli,
)
from google import genai
from google.genai import types

logger = logging.getLogger("ai-worker")
logger.setLevel(logging.INFO)

async def process_audio_stream(audio_stream: rtc.AudioStream, gemini_session):
    logger.info("Started streaming audio from LiveKit to Gemini")
    try:
        async for frame_event in audio_stream:
            audio_bytes = frame_event.frame.data.tobytes()
            await gemini_session.send_realtime_input(
                media=types.Blob(data=audio_bytes, mime_type="audio/pcm;rate=16000")
            )
    except Exception as e:
        logger.error(f"Error reading audio stream: {e}", exc_info=True)
    finally:
        logger.info("Audio stream to Gemini closed")

async def process_gemini_responses(gemini_session, audio_source: rtc.AudioSource):
    logger.info("Started receiving responses from Gemini to LiveKit")
    try:
        async for msg in gemini_session.receive():
            server_content = msg.server_content
            if server_content is not None:
                model_turn = server_content.model_turn
                if model_turn:
                    for part in model_turn.parts:
                        if part.inline_data and part.inline_data.data:
                            audio_bytes = part.inline_data.data
                            # Gemini returns 24kHz PCM by default
                            frame = rtc.AudioFrame(
                                data=audio_bytes,
                                sample_rate=24000,
                                num_channels=1,
                                samples_per_channel=len(audio_bytes) // 2
                            )
                            await audio_source.capture_frame(frame)
    except Exception as e:
        logger.error(f"Error processing Gemini response: {e}", exc_info=True)
    finally:
        logger.info("Gemini response loop closed")

async def entrypoint(ctx: JobContext):
    logger.info("Starting raw Gemini AI Worker...")
    try:
        await ctx.connect()
    except Exception as e:
        logger.error(f"Error in connect: {e}", exc_info=True)
        raise

    try:
        gemini_api_key = os.environ.get("GEMINI_API_KEY")
        if not gemini_api_key:
            raise ValueError("GEMINI_API_KEY environment variable not set")

        client = genai.Client(api_key=gemini_api_key)
        
        # Audio source for AI responses
        audio_source = rtc.AudioSource(sample_rate=24000, num_channels=1)
        audio_track = rtc.LocalAudioTrack.create_audio_track("ai-mic", audio_source)
        options = rtc.TrackPublishOptions(source=rtc.TrackSource.SOURCE_MICROPHONE)
        await ctx.room.local_participant.publish_track(audio_track, options)
        
        # We must set response_modalities to AUDIO so it returns audio inline_data
        config = {
            "response_modalities": ["AUDIO"],
            "system_instruction": types.Content(
                parts=[types.Part.from_text(text="You are a helpful call center assistant. Please answer concisely and politely in Arabic. Introduce yourself briefly.")]
            )
        }
        
        logger.info("Connecting to Gemini Live API...")
        async with client.aio.live.connect(model="gemini-3.1-flash-live-preview", config=config) as session:
            logger.info("Connected to Gemini Live API!")
            
            # Send an initial greeting to trigger the AI to start speaking
            await session.send(input="مرحبا", end_of_turn=True)
            
            asyncio.create_task(process_gemini_responses(session, audio_source))

            @ctx.room.on("track_subscribed")
            def on_track_subscribed(track: rtc.Track, publication: rtc.RemoteTrackPublication, participant: rtc.RemoteParticipant):
                if track.kind == rtc.TrackKind.KIND_AUDIO:
                    logger.info(f"Subscribed to audio track from {participant.identity}")
                    # Request 16000Hz from LiveKit as Gemini expects
                    audio_stream = rtc.AudioStream.from_track(track=track, sample_rate=16000, num_channels=1)
                    asyncio.create_task(process_audio_stream(audio_stream, session))

            # Subscribe to existing tracks if any
            for participant in ctx.room.remote_participants.values():
                for pub in participant.track_publications.values():
                    if pub.track and pub.track.kind == rtc.TrackKind.KIND_AUDIO:
                        logger.info(f"Subscribed to existing audio track from {participant.identity}")
                        audio_stream = rtc.AudioStream.from_track(track=pub.track, sample_rate=16000, num_channels=1)
                        asyncio.create_task(process_audio_stream(audio_stream, session))

            # Keep the entrypoint alive as long as we are connected
            while ctx.room.connection_state == rtc.ConnectionState.CONN_CONNECTED:
                await asyncio.sleep(1)

    except Exception as e:
        logger.error(f"Error in agent setup: {e}", exc_info=True)
        raise

if __name__ == "__main__":
    cli.run_app(WorkerOptions(entrypoint_fnc=entrypoint))

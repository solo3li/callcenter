import asyncio
from livekit.agents import Agent
from livekit.plugins import google

agent = Agent(
    llm=google.beta.realtime.RealtimeModel(
        model="gemini-3.1-flash-live-preview",
        voice="Puck",
        instructions="You are a helpful assistant."
    )
)
print("Agent created:", agent)
print("start method exists:", hasattr(agent, 'start'))

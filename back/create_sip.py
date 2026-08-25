import os
import asyncio
from livekit import api
import uuid

async def main():
    lkapi = api.LiveKitAPI(
        os.environ.get("LIVEKIT_URL", "http://livekit:7880"),
        os.environ.get("LIVEKIT_API_KEY", "devkey"),
        os.environ.get("LIVEKIT_API_SECRET", "secret")
    )
    
    try:
        trunk = await lkapi.sip.create_sip_trunk(
            api.CreateSIPTrunkRequest(
                inbound_addresses=["0.0.0.0/0"],
                inbound_numbers_regex=[".*"],
                name="Asterisk",
            )
        )
        print("Trunk created:", trunk)
    except Exception as e:
        print("Trunk error (may already exist):", e)

    try:
        unique_prefix = f"call_{uuid.uuid4().hex[:8]}"
        rule = await lkapi.sip.create_sip_dispatch_rule(
            api.CreateSIPDispatchRuleRequest(
                name="DynamicInbound",
                rule=api.SIPDispatchRule(
                    dispatch_rule_direct=api.SIPDispatchRuleDirect(
                        room_name=unique_prefix
                    )
                )
            )
        )
        print(f"Dispatch rule created with room prefix: {unique_prefix}")
        print("Add SIP_DISPATCH_ROOM_PREFIX to your env to reference this")
    except Exception as e:
        print("Dispatch rule error (may already exist):", e)

    try:
        catchall = await lkapi.sip.create_sip_dispatch_rule(
            api.CreateSIPDispatchRuleRequest(
                name="CatchAll",
                rule=api.SIPDispatchRule(
                    dispatch_rule_direct=api.SIPDispatchRuleDirect(
                        room_name="sip-inbound"
                    )
                )
            )
        )
        print("CatchAll dispatch rule created:", catchall)
    except Exception as e:
        print("CatchAll rule error (may already exist):", e)
        
    await lkapi.aclose()

if __name__ == "__main__":
    asyncio.run(main())
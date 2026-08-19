import os
import asyncio
from livekit import api

async def main():
    lkapi = api.LiveKitAPI("http://livekit:7880", "devkey", "secret")
    
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
        print("Trunk error:", e)

    try:
        rule = await lkapi.sip.create_sip_dispatch_rule(
            api.CreateSIPDispatchRuleRequest(
                name="Default",
                rule=api.SIPDispatchRule(
                    dispatch_rule_direct=api.SIPDispatchRuleDirect(room_name="sip-room")
                )
            )
        )
        print("Dispatch rule created:", rule)
    except Exception as e:
        print("Rule error:", e)
        
    await lkapi.aclose()

asyncio.run(main())

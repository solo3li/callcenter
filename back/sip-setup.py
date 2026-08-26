"""LiveKit-SIP bootstrap for the v0 inbound + transfer topology.

Creates (idempotently, matched by name):
  1. Inbound trunk   - IP allow-listed to the customer PBX, matching configured DIDs.
  2. Dispatch rule   - individual rooms with owner-scoped prefix "call_u{OWNER_USER_ID}".
  3. Outbound trunk  - targets the customer PBX for destination transfers.

Run inside the compose network:
  PBX_IP=10.0.0.5 OWNER_USER_ID=<users.id guid> [SIP_DIDS=+15551234567,+15557654321] \
    docker compose --profile setup run --rm sip-setup
"""
import asyncio
import os
import re
import sys
import uuid

from livekit import api

INBOUND_TRUNK_NAME = "inbound-pbx"
OUTBOUND_TRUNK_NAME = "outbound-pbx"
DISPATCH_RULE_NAME = "inbound-rooms"


def env(name: str, default: str = "") -> str:
    return os.environ.get(name, default)


async def main() -> int:
    pbx_ip = env("PBX_IP")
    owner_user_id = env("OWNER_USER_ID")
    dids = [d.strip() for d in env("SIP_DIDS").split(",") if d.strip()]

    if not pbx_ip:
        print("ERROR: PBX_IP is required (customer PBX address).")
        return 1
    try:
        uuid.UUID(owner_user_id)
    except ValueError:
        print("ERROR: OWNER_USER_ID must be a valid users.id GUID.")
        return 1

    room_prefix = f"call_u{owner_user_id}"

    lkapi = api.LiveKitAPI(
        url=env("LIVEKIT_URL", "http://livekit:7880"),
        api_key=env("LIVEKIT_API_KEY", "devkey"),
        api_secret=env("LIVEKIT_API_SECRET", "secret"),
    )

    try:
        inbound_sid = outbound_sid = rule_id = None

        # ── 1. Inbound trunk ────────────────────────────────────────────
        existing_trunks = await lkapi.sip.list_sip_inbound_trunk(api.ListSIPInboundTrunkRequest())
        for t in existing_trunks.items:
            if t.name == INBOUND_TRUNK_NAME:
                inbound_sid = t.sip_trunk_id
                break
        if inbound_sid:
            print(f"[ok] inbound trunk exists: {INBOUND_TRUNK_NAME} ({inbound_sid})")
        else:
            numbers = dids or []
            trunk = await lkapi.sip.create_sip_inbound_trunk(
                api.CreateSIPInboundTrunkRequest(
                    name=INBOUND_TRUNK_NAME,
                    numbers=numbers,
                    allowed_addresses=[f"{pbx_ip}/32"],
                )
            )
            inbound_sid = trunk.sip_trunk_id
            print(f"[created] inbound trunk {INBOUND_TRUNK_NAME}: {inbound_sid}")

        # ── 2. Dispatch rule (owner-scoped individual rooms) ───────────
        existing_rules = await lkapi.sip.list_sip_dispatch_rule(api.ListSIPDispatchRuleRequest())
        for r in existing_rules.items:
            if r.name == DISPATCH_RULE_NAME:
                rule_id = r.sip_dispatch_rule_id
                break
        if rule_id:
            print(f"[ok] dispatch rule exists: {DISPATCH_RULE_NAME} ({rule_id})")
        else:
            rule = await lkapi.sip.create_sip_dispatch_rule(
                api.CreateSIPDispatchRuleRequest(
                    name=DISPATCH_RULE_NAME,
                    trunk_ids=[inbound_sid],
                    rule=api.SIPDispatchRule(
                        dispatch_rule_individual=api.SIPDispatchRuleIndividual(
                            room_prefix=room_prefix,
                            pin="",
                        )
                    ),
                )
            )
            rule_id = rule.sip_dispatch_rule_id
            print(f"[created] dispatch rule {DISPATCH_RULE_NAME}: {rule_id}")
            print(f"          rooms will be named {room_prefix}<random>")

        # ── 3. Outbound trunk (destination transfers) ──────────────────
        existing_out = await lkapi.sip.list_sip_outbound_trunk(api.ListSIPOutboundTrunkRequest())
        for t in existing_out.items:
            if t.name == OUTBOUND_TRUNK_NAME:
                outbound_sid = t.sip_trunk_id
                break
        if outbound_sid:
            print(f"[ok] outbound trunk exists: {OUTBOUND_TRUNK_NAME} ({outbound_sid})")
        else:
            trunk = await lkapi.sip.create_sip_trunk(
                api.CreateSIPOutboundTrunkRequest(
                    name=OUTBOUND_TRUNK_NAME,
                    address=f"{pbx_ip}:5060",
                    numbers_to=dids or ["0000"],
                    transport=api.SIPTransport.SIP_TRANSPORT_UDP,
                )
            )
            outbound_sid = trunk.sip_trunk_id
            print(f"[created] outbound trunk {OUTBOUND_TRUNK_NAME}: {outbound_sid}")

        print("\n──── summary ────")
        print(f"inbound_trunk_id={inbound_sid}")
        print(f"dispatch_rule_id={rule_id}")
        print(f"LIVEKIT_OUTBOUND_TRUNK_ID={outbound_sid}")
        print(f"room_prefix={room_prefix}")
        if not dids:
            print("\nNOTE: SIP_DIDS was empty; inbound accepts any DID and "
                  "outbound uses placeholder '0000'. Re-run with real DIDs.")
        return 0
    finally:
        await lkapi.aclose()


if __name__ == "__main__":
    try:
        sys.exit(asyncio.run(main()))
    except KeyboardInterrupt:
        sys.exit(130)

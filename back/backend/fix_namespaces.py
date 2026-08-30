import os
import re

base_dir = r"c:\Users\solo\Desktop\testviop\back\backend"

mapping = {
    "Identity": {
        "Dtos": ["AuthDtos.cs", "ApiKeyDtos.cs", "LicenseDtos.cs", "PartnerDtos.cs", "HumanAgentDtos.cs"],
        "Endpoints": ["AuthEndpoints.cs", "ApiKeyEndpoints.cs", "LicenseEndpoints.cs", "PartnerEndpoints.cs", "HumanAgentEndpoints.cs"]
    },
    "Billing": {
        "Dtos": ["PlanDtos.cs", "SubscriptionDtos.cs", "UsageDtos.cs"],
        "Endpoints": ["PlanEndpoints.cs", "SubscriptionEndpoints.cs", "UsageEndpoints.cs"]
    },
    "CallOperations": {
        "Dtos": ["CallSessionDtos.cs", "CallTransferDtos.cs", "CallHandoffDtos.cs", "CallRecordingDtos.cs"],
        "Endpoints": ["CallSessionEndpoints.cs", "CallTransferEndpoints.cs", "CallHandoffEndpoints.cs", "CallRecordingEndpoints.cs", "LiveKitEndpoints.cs", "LiveKitWebhookEndpoints.cs", "SipDestinationEndpoints.cs", "WebhookEndpoints.cs"]
    },
    "Configuration": {
        "Dtos": ["ActionDtos.cs", "CallConfigDtos.cs", "KnowledgeBaseDtos.cs", "PersonaDtos.cs", "WorkflowDtos.cs"],
        "Endpoints": ["ActionEndpoints.cs", "CallConfigurationEndpoints.cs", "KnowledgeBaseEndpoints.cs", "PersonaEndpoints.cs", "WorkflowEndpoints.cs"]
    },
    "Analytics": {
        "Dtos": ["StatsDtos.cs"],
        "Endpoints": ["StatsEndpoints.cs"]
    }
}

for module, categories in mapping.items():
    for category, files in categories.items():
        for file in files:
            dst = os.path.join(base_dir, "Modules", module, category, file)
            if os.path.exists(dst):
                with open(dst, 'r', encoding='utf-8') as f:
                    content = f.read()
                
                # Replace both block-scoped and file-scoped forms
                if category == "Dtos":
                    content = re.sub(r'namespace\s+backend\.Dtos\b', f'namespace backend.Modules.{module}.Dtos', content)
                elif category == "Endpoints":
                    content = re.sub(r'namespace\s+backend\.Endpoints\b', f'namespace backend.Modules.{module}.Endpoints', content)
                
                with open(dst, 'w', encoding='utf-8') as f:
                    f.write(content)

legacy_dst = os.path.join(base_dir, "Infrastructure", "Endpoints", "LegacyShimsEndpoints.cs")
if os.path.exists(legacy_dst):
    with open(legacy_dst, 'r', encoding='utf-8') as f:
        content = f.read()
    content = re.sub(r'namespace\s+backend\.Endpoints\b', 'namespace backend.Infrastructure.Endpoints', content)
    with open(legacy_dst, 'w', encoding='utf-8') as f:
        f.write(content)

# Additionally, replace `using backend.Dtos` without semicolon across all files in case it's in a using block inside a namespace
new_dto_usings = "\n".join([f"using backend.Modules.{module}.Dtos;" for module in mapping.keys()])
new_endpoint_usings = "\n".join([f"using backend.Modules.{module}.Endpoints;" for module in mapping.keys()]) + "\nusing backend.Infrastructure.Endpoints;"

for root, _, files in os.walk(base_dir):
    if "obj" in root or "bin" in root or ".git" in root:
        continue
    for file in files:
        if file.endswith(".cs"):
            filepath = os.path.join(root, file)
            with open(filepath, 'r', encoding='utf-8') as f:
                try:
                    content = f.read()
                except UnicodeDecodeError:
                    continue
            
            modified = False
            
            if "using backend.Dtos\n" in content or "using backend.Dtos\r\n" in content:
                content = re.sub(r'using\s+backend\.Dtos\s*;?', new_dto_usings, content)
                modified = True
                
            if "using backend.Endpoints\n" in content or "using backend.Endpoints\r\n" in content:
                content = re.sub(r'using\s+backend\.Endpoints\s*;?', new_endpoint_usings, content)
                modified = True
                
            if modified:
                with open(filepath, 'w', encoding='utf-8') as f:
                    f.write(content)
print("Namespace fixes applied.")

import os
import shutil
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

# 1. Create directories
for module in mapping.keys():
    os.makedirs(os.path.join(base_dir, "Modules", module, "Dtos"), exist_ok=True)
    os.makedirs(os.path.join(base_dir, "Modules", module, "Endpoints"), exist_ok=True)

os.makedirs(os.path.join(base_dir, "Infrastructure", "Endpoints"), exist_ok=True)

# 2. Move files and update their namespaces
for module, categories in mapping.items():
    for category, files in categories.items():
        for file in files:
            src = os.path.join(base_dir, category, file)
            dst = os.path.join(base_dir, "Modules", module, category, file)
            
            if os.path.exists(src):
                # Read content before move
                with open(src, 'r', encoding='utf-8') as f:
                    content = f.read()
                
                # Update namespace
                old_ns = f"namespace backend.{category};"
                new_ns = f"namespace backend.Modules.{module}.{category};"
                content = content.replace(old_ns, new_ns)
                
                # Write to destination
                with open(dst, 'w', encoding='utf-8') as f:
                    f.write(content)
                
                # Remove source
                os.remove(src)

# Move LegacyShimsEndpoints.cs
legacy_src = os.path.join(base_dir, "Endpoints", "LegacyShimsEndpoints.cs")
legacy_dst = os.path.join(base_dir, "Infrastructure", "Endpoints", "LegacyShimsEndpoints.cs")
if os.path.exists(legacy_src):
    with open(legacy_src, 'r', encoding='utf-8') as f:
        content = f.read()
    content = content.replace("namespace backend.Endpoints;", "namespace backend.Infrastructure.Endpoints;")
    with open(legacy_dst, 'w', encoding='utf-8') as f:
        f.write(content)
    os.remove(legacy_src)

# 3. Update all files referencing old namespaces
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
            
            if "using backend.Dtos;" in content:
                content = content.replace("using backend.Dtos;", new_dto_usings)
                modified = True
                
            if "using backend.Endpoints;" in content:
                content = content.replace("using backend.Endpoints;", new_endpoint_usings)
                modified = True
                
            if modified:
                with open(filepath, 'w', encoding='utf-8') as f:
                    f.write(content)

print("Migration completed successfully.")

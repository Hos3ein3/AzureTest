#!/usr/bin/env python3
"""
TrackerPro Version Bump Script
Increments or sets the AppVersion across appsettings*.json in src/AzureTest.MVC.
Usage:
    python3 scripts/bump_version.py            # Bumps patch/minor: V3.1 -> V3.2
    python3 scripts/bump_version.py V3.5       # Explicitly sets version to V3.5
"""

import json
import os
import re
import sys

APP_SETTINGS_FILES = [
    "src/AzureTest.MVC/appsettings.json",
    "src/AzureTest.MVC/appsettings.Development.json",
    "src/AzureTest.MVC/appsettings.ProductionSimulateLocal.json"
]

def bump_string(version_str: str) -> str:
    match = re.match(r"^([vV]?)(\d+(?:\.\d+)*)$", version_str.strip())
    if not match:
        return version_str + ".1"
    
    prefix = match.group(1) or "V"
    parts = [int(p) for p in match.group(2).split(".")]
    parts[-1] += 1
    return f"{prefix}{'.'.join(str(p) for p in parts)}"

def main():
    repo_root = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    target_files = [os.path.join(repo_root, f) for f in APP_SETTINGS_FILES]

    primary_file = target_files[0]
    if not os.path.exists(primary_file):
        print(f"Error: {primary_file} not found.")
        sys.exit(1)

    with open(primary_file, "r", encoding="utf-8") as f:
        data = json.load(f)

    current_version = data.get("AppVersion", "V3.1")

    if len(sys.argv) > 1 and sys.argv[1].strip():
        new_version = sys.argv[1].strip()
        if not new_version.upper().startswith("V"):
            new_version = f"V{new_version}"
    else:
        new_version = bump_string(current_version)

    print(f"Bumping TrackerPro AppVersion: {current_version} -> {new_version}")

    for file_path in target_files:
        if not os.path.exists(file_path):
            continue
        try:
            with open(file_path, "r", encoding="utf-8") as f:
                content = json.load(f)
            
            # Put AppVersion at the very top of the dictionary
            updated = {"AppVersion": new_version}
            for k, v in content.items():
                if k != "AppVersion":
                    updated[k] = v

            with open(file_path, "w", encoding="utf-8") as f:
                json.dump(updated, f, indent=2)
                f.write("\n")
            print(f"  Updated {os.path.relpath(file_path, repo_root)}")
        except Exception as e:
            print(f"  Failed to update {file_path}: {e}")

if __name__ == "__main__":
    main()

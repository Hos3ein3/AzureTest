#!/usr/bin/env python3
"""
TrackerPro Version Bump Script
Displays current version, prompts for or automatically increments AppVersion across appsettings*.json in src/AzureTest.MVC.

Usage:
    python3 scripts/bump_version.py            # Interactive: shows current version & prompts for new version (default: auto-increment)
    python3 scripts/bump_version.py 3.2        # Explicitly sets version to 3.2 or V3.2
    python3 scripts/bump_version.py --auto     # Non-interactive: auto-increments without prompting (used by CI / git hooks)
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
    
    prefix = match.group(1) or ""
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
    suggested_version = bump_string(current_version)

    # Filter arguments
    explicit_args = [a for a in sys.argv[1:] if a not in ("--auto", "-y", "-a")]
    is_auto = "--auto" in sys.argv or "-y" in sys.argv or "-a" in sys.argv or not sys.stdin.isatty()

    if explicit_args:
        new_version = explicit_args[0].strip()
    elif is_auto:
        new_version = suggested_version
    else:
        # Interactive mode: show current version and ask for new version
        print(f"\nCurrent TrackerPro Version: {current_version}")
        try:
            prompt_text = f"Enter new version (Press Enter for '{suggested_version}'): "
            user_input = input(prompt_text).strip()
            new_version = user_input if user_input else suggested_version
        except (KeyboardInterrupt, EOFError):
            print("\nAborted.")
            sys.exit(0)

    print(f"\nSetting TrackerPro AppVersion: {current_version} -> {new_version}")

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

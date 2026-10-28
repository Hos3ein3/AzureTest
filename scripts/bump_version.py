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

def get_version_suggestions(version_str: str):
    match = re.match(r"^([vV]?)(\d+(?:\.\d+)*)$", version_str.strip())
    if not match:
        fallback = version_str + ".1"
        return {"patch": fallback, "minor": fallback, "major": fallback}
    
    prefix = match.group(1) or ""
    parts = [int(p) for p in match.group(2).split(".")]
    
    if len(parts) == 1:
        return {
            "patch": f"{prefix}{parts[0]}.0.1",
            "minor": f"{prefix}{parts[0]}.1",
            "major": f"{prefix}{parts[0] + 1}.0"
        }
    elif len(parts) == 2:
        return {
            "patch": f"{prefix}{parts[0]}.{parts[1]}.1",
            "minor": f"{prefix}{parts[0]}.{parts[1] + 1}",
            "major": f"{prefix}{parts[0] + 1}.0"
        }
    else:
        return {
            "patch": f"{prefix}{parts[0]}.{parts[1]}.{parts[2] + 1}",
            "minor": f"{prefix}{parts[0]}.{parts[1] + 1}",
            "major": f"{prefix}{parts[0] + 1}.0"
        }

def bump_string(version_str: str) -> str:
    sugg = get_version_suggestions(version_str)
    # Default is always patching (e.g. 3.4 -> 3.4.1, 3.4.1 -> 3.4.2)
    return sugg["patch"]

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

    if "--current" in sys.argv or "-c" in sys.argv:
        print(f"Current TrackerPro Version: {current_version}")
        sys.exit(0)

    # Filter arguments
    explicit_args = [a for a in sys.argv[1:] if a not in ("--auto", "-y", "-a", "--current", "-c") and not a.startswith("--")]
    is_auto = "--auto" in sys.argv or "-y" in sys.argv or "-a" in sys.argv or not sys.stdin.isatty()

    if explicit_args:
        new_version = explicit_args[0].strip()
    elif is_auto:
        new_version = suggested_version
    else:
        # Interactive mode: show current version and ask for new version
        sugg = get_version_suggestions(current_version)
        default_version = sugg["patch"]
        print(f"\nCurrent TrackerPro Version: {current_version}")
        print(f"Suggestions: [Patch (Default): {sugg['patch']} | Minor: {sugg['minor']} | Major: {sugg['major']}]")
        try:
            prompt_text = f"Enter new version (Press Enter for default '{default_version}', or type e.g. '{sugg['minor']}', '4.0'): "
            user_input = input(prompt_text).strip()
            if user_input.lower() in ("patch", "p"):
                new_version = sugg["patch"]
            elif user_input.lower() in ("minor", "m"):
                new_version = sugg["minor"]
            elif user_input.lower() in ("major", "maj"):
                new_version = sugg["major"]
            elif user_input:
                new_version = user_input
            else:
                new_version = default_version
        except (KeyboardInterrupt, EOFError):
            print("\n[TrackerPro] Version update aborted.")
            sys.exit(1)

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

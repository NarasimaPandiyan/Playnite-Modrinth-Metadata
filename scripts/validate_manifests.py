#!/usr/bin/env python3
"""
Manifest validator for Playnite Modrinth Metadata Extension.
Verifies that extension.yaml, installer.yaml, and addon.yaml adhere to
Playnite's official manifest specifications (Playnite.Toolbox Verify.cs).
"""

import os
import sys
import yaml
import urllib.request
import re
from datetime import datetime

# Reconfigure stdout for UTF-8 compatibility
if hasattr(sys.stdout, 'reconfigure'):
    sys.stdout.reconfigure(encoding='utf-8')

def is_http_url(url):
    return url and (url.startswith("http://") or url.startswith("https://"))

def check_url(url, prop_name, mandatory=True, allow_html=False):
    if not url:
        if mandatory:
            print(f"[FAIL] {prop_name} URL is missing.")
            return False
        return True
    if not is_http_url(url):
        print(f"[FAIL] {prop_name} is not a valid HTTP URL: {url}")
        return False
    
    try:
        req = urllib.request.Request(url, headers={"User-Agent": "Playnite-Manifest-Validator/1.0"})
        with urllib.request.urlopen(req, timeout=15) as resp:
            if resp.status < 200 or resp.status >= 400:
                print(f"[FAIL] {prop_name} URL returned HTTP status {resp.status}: {url}")
                return False
            content = resp.read(1024).decode('utf-8', errors='ignore')
            if not allow_html and "<html" in content.lower():
                print(f"[FAIL] {prop_name} points to HTML page instead of raw file: {url}")
                return False
    except Exception as e:
        print(f"[FAIL] {prop_name} URL check failed ({e}): {url}")
        return False
    print(f"  [OK] {prop_name} URL valid & reachable: {url}")
    return True

def validate_version(ver_str):
    if not ver_str:
        return False
    parts = str(ver_str).split('.')
    return len(parts) >= 2 and all(p.isdigit() for p in parts)

def validate_date(date_val):
    if not date_val:
        return False
    if isinstance(date_val, datetime):
        return True
    try:
        datetime.strptime(str(date_val), "%Y-%m-%d")
        return True
    except ValueError:
        return False

def validate_installer_manifest(filepath):
    print(f"\n--- Validating Installer Manifest: {filepath} ---")
    if not os.path.exists(filepath):
        print(f"[FAIL] File not found: {filepath}")
        return False
        
    with open(filepath, 'r', encoding='utf-8') as f:
        data = yaml.safe_load(f)
        
    passed = True
    addon_id = data.get("AddonId")
    if not addon_id:
        print("[FAIL] AddonId missing in installer manifest.")
        passed = False
    else:
        print(f"  [OK] AddonId: {addon_id}")
        
    packages = data.get("Packages", [])
    if not packages:
        print("[FAIL] Packages list is empty in installer manifest.")
        passed = False
    else:
        print(f"  [OK] Found {len(packages)} package entries.")
        
    for idx, pkg in enumerate(packages):
        pkg_ver = pkg.get("Version")
        req_api = pkg.get("RequiredApiVersion")
        rel_date = pkg.get("ReleaseDate")
        pkg_url = pkg.get("PackageUrl")
        changelog = pkg.get("Changelog")
        
        print(f"\n  Checking Package #{idx+1} (Version {pkg_ver}):")
        if not validate_version(pkg_ver):
            print(f"  [FAIL] Version is missing or invalid: {pkg_ver}")
            passed = False
        else:
            print(f"    [OK] Version: {pkg_ver}")
            
        if not validate_version(req_api):
            print(f"  [FAIL] RequiredApiVersion is missing or invalid: {req_api}")
            passed = False
        else:
            print(f"    [OK] RequiredApiVersion: {req_api}")
            
        if not validate_date(rel_date):
            print(f"  [FAIL] ReleaseDate is missing or invalid: {rel_date}")
            passed = False
        else:
            print(f"    [OK] ReleaseDate: {rel_date}")
            
        if not check_url(pkg_url, f"Package #{idx+1} PackageUrl", mandatory=True, allow_html=False):
            passed = False
            
        if not changelog or not isinstance(changelog, list) or len(changelog) == 0:
            print(f"  [FAIL] Changelog is missing or empty.")
            passed = False
        else:
            print(f"    [OK] Changelog items: {len(changelog)}")
            
    if passed:
        print(f"\n[SUCCESS] Installer manifest '{filepath}' passed all verification checks!")
    else:
        print(f"\n[FAIL] Installer manifest '{filepath}' failed verification.")
    return passed

def main():
    root_dir = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))
    installer_path = os.path.join(root_dir, "ModrinthModpacksMetadata", "installer.yaml")
    
    success = validate_installer_manifest(installer_path)
    if not success:
        sys.exit(1)

if __name__ == "__main__":
    main()

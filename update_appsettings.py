import json
import glob
import os

files = glob.glob('**/appsettings*.json', recursive=True)

for file in files:
    if 'bin' in file or 'obj' in file:
        continue
    with open(file, 'r') as f:
        try:
            data = json.load(f)
        except json.JSONDecodeError:
            continue
    
    modified = False
    
    if 'Logging' in data and 'LogLevel' in data['Logging']:
        if 'Microsoft.Extensions.Diagnostics.HealthChecks' not in data['Logging']['LogLevel']:
            data['Logging']['LogLevel']['Microsoft.Extensions.Diagnostics.HealthChecks'] = 'Warning'
            modified = True
        elif data['Logging']['LogLevel']['Microsoft.Extensions.Diagnostics.HealthChecks'] != 'Warning':
            data['Logging']['LogLevel']['Microsoft.Extensions.Diagnostics.HealthChecks'] = 'Warning'
            modified = True
            
        if 'HealthChecks.UI' not in data['Logging']['LogLevel']:
            data['Logging']['LogLevel']['HealthChecks.UI'] = 'Warning'
            modified = True
        elif data['Logging']['LogLevel']['HealthChecks.UI'] != 'Warning':
            data['Logging']['LogLevel']['HealthChecks.UI'] = 'Warning'
            modified = True
            
    if 'HealthChecksUI' in data:
        data['HealthChecksUI']['EvaluationTimeInSeconds'] = 3600
        data['HealthChecksUI']['MinimumSecondsBetweenFailureNotifications'] = 3600
        modified = True
        
    if modified:
        with open(file, 'w') as f:
            json.dump(data, f, indent=2)
        print(f"Updated {file}")


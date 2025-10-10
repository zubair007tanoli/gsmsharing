# Cursor Settings Reset Instructions

## Steps to Reset Cursor Settings:

1. **Close Cursor completely**

2. **Navigate to Cursor settings folder:**
   - Windows: `C:\Users\zubai\AppData\Roaming\Cursor`
   - Look for folders like:
     - `User`
     - `Cache`
     - `CachedData`
     - `logs`

3. **Backup current settings:**
   - Create a backup folder: `Cursor_Backup_[date]`
   - Copy all folders to the backup location

4. **Delete or rename these folders:**
   - `Cache`
   - `CachedData`
   - `GPUCache`
   - `logs`

5. **Restart Cursor**
   - Cursor will recreate these folders with default settings

6. **If the issue persists, also reset user settings:**
   - Rename the `User` folder to `User_backup`
   - Restart Cursor again

## Additional Troubleshooting:

### Check for corrupted workspace settings:
- In your project folder, look for `.cursor` or `.vscode` folders
- Temporarily rename them to test if they're causing issues

### Command line reset (if GUI is not working):
```powershell
# Kill all Cursor processes
taskkill /F /IM cursor.exe

# Clear npm cache (if using Node.js projects)
npm cache clean --force

# Clear Cursor cache
Remove-Item -Path "$env:APPDATA\Cursor\Cache" -Recurse -Force
Remove-Item -Path "$env:APPDATA\Cursor\CachedData" -Recurse -Force
```

### Report the issue:
If none of these solutions work, report the issue to Cursor support with:
- The full error message
- Your Cursor version
- OS version (Windows, based on your path)
- Recent actions before the error occurred
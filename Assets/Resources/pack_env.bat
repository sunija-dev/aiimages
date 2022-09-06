@ECHO [off]
call C:\Users\Sunija/anaconda3/Scripts/activate.bat
conda pack -n ldm -o C:/Data/simplestable/Assets/../deploy\env.zip --ignore-missing-files --ignore-editable-packages --format zip
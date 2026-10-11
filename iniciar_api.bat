@echo off 
cls 
cd API 
python -m pip install -r requirements.txt 
start uvicorn conexionApi:app --reload 
pause 

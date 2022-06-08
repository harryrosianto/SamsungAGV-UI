import socket
import fastapi
from starlette.routing import Host
import uvicorn
from pydantic import BaseModel
from fastapi.responses import JSONResponse
from fastapi.middleware.cors import CORSMiddleware
import json
import os
from time import sleep
from datetime import datetime
from fastapi import FastAPI
from fastapi.staticfiles import StaticFiles
import requests as req2
import threading

class Data(BaseModel):
    value: list

class ReqModel(BaseModel):
    command: str
    serialNumber: int

class CaseQueueModel(BaseModel):
    station:str
    caseName:str
    jobId:str

arrayOfCaseQueue = []
# f = open("C:\\0.root\\2.dev\\arrayCaseQueue.txt","r")
# arrayOfCaseQueue = json.loads(f.read())
# f.close()

app = fastapi.FastAPI()
app.add_middleware(
    CORSMiddleware,
    allow_origins=['*'],
    allow_credentials=True,
    allow_methods=["*"],
    allow_headers=["*"],
)
# app.mount("/static",StaticFiles(directory="D:\\buildNode"),name="build")
# app.mount("/static", StaticFiles(directory="static"), name="static")
cnt =0

resMissionGetActiveList = ""
flagStopThread = False
def funcMissionGetActiveList():
    while True:
        getMissionAgv = 'http://localhost:30111/command?cmd=missionC.missionGetActiveList()'
        global resMissionGetActiveList
        res = req2.get(getMissionAgv)
        resMissionGetActiveList = res.text
        now = datetime.now()
        print("GET Mission Data OK ", now)
        if flagStopThread:
            break
        # print(resMissionGetActiveList)

try:
    
    cnt+1
    sleep(15)
    cl = socket.socket(family=socket.AF_INET, type=socket.SOCK_DGRAM)
    ser = ("127.0.0.1", 30111)

    cl.sendto(b'{"command":"devC.getCarList()","serialNumber": 1}0000', ser)
    msg = cl.recvfrom(8192)
    output2=msg[0].decode('utf-8')
    print("Counter : ",cnt)
    print(output2)

    @app.post('/req')
    async def request(cmd: ReqModel):
        
        # if (cmd.command[0:25] == "missionC.netMissionCancel"):
        #     data = 'http://localhost:30111/command?cmd=' + str(cmd.command)

        #     r = req2.get(data)
        #     # print(r.text)
        #     dataSplit  = str(r.text).split("errMark")
        #     dataErrMark = '"errMark' + dataSplit[1].split(',')[0]
        #     dataMsg = dataSplit[0][1:-1]
        #     dataCommand = '"command": "' + cmd.command

        #     dataToSend = "{\n" + dataErrMark + "," + dataMsg + dataCommand + "\n}"

        #     jsondata =  json.loads(dataToSend)

        if (cmd.command == "missionC.missionGetActiveList()"):
            data = 'http://localhost:30111/command?cmd=' + str(cmd.command)

            # r = req2.get(data)
            # thread.start()
            # print(r.text)
            global resMissionGetActiveList
            dataSplit = str(resMissionGetActiveList).split("errMark")
            dataErrMark = '"errMark' + dataSplit[1].split(',')[0]
            dataMsg = dataSplit[0][1:-1]
            dataCommand = '"command": "missionC.missionGetActiveList()"'

            dataToSend = "{\n" + dataErrMark + "," + dataMsg + dataCommand + "\n}"

            # jsondata =  json.loads(dataToSend)
            # jsondata = dataToSend
            # jsondata['command'] = cmd.command
            # print(dataToSend)
            # data = 'http://localhost:30111/command?cmd=' + str(cmd.command)
            # r = req2.get(data)
            # print(r.text)
            jsondata =  json.loads(dataToSend)
            
        else:
            req = cmd.json()
            req+="0000"
            cl.sendto(req.encode('utf-8'), ser)
            # writeLog()
            msg = cl.recvfrom(8192)
            output1=msg[0]
            output2=msg[0].decode('utf-8')
            output2 = output2[:-4]
            jsondata =  json.loads(output2)
            jsondata['command'] = cmd.command
        return JSONResponse(content=jsondata)

    @app.post('/addQueue')
    async def request(cmd: CaseQueueModel):
        reqBody=cmd.json()
        print(reqBody)
        reqBodyJson = json.loads(reqBody)
        if(reqBodyJson not in arrayOfCaseQueue):
            arrayOfCaseQueue.append(json.loads(reqBody))
            # f = open("C:\\0.root\\2.dev\\arrayCaseQueue.txt","w")
            # f.write(json.dumps(arrayOfCaseQueue))
            # f.close()
            return(JSONResponse(content={"status":"OK"}))
        else:
            return(JSONResponse(content={"status":"NOT OK"}))
        

    @app.post('/getQueue')
    async def request():
        return JSONResponse(content=arrayOfCaseQueue)

    @app.get('/getQueueRaw')
    async def request():
        return JSONResponse(content=arrayOfCaseQueue)

    @app.post('/deleteQueue')
    async def request(cmd: CaseQueueModel):
        reqBody = cmd.json()
        print(reqBody)
        queueToDelete = json.loads(reqBody)
        if(queueToDelete in arrayOfCaseQueue):
            arrayOfCaseQueue.remove(queueToDelete)
        # f = open("C:\\0.root\\2.dev\\arrayCaseQueue.txt","w")
        # f.write(json.dumps(arrayOfCaseQueue))
        # f.close()
        return(JSONResponse(content={"status":"OK"}))

except socket.timeout:
    print("timeout exception")
    
if __name__ == '__main__':
    print("Running on port 8000")
    thread = threading.Thread(target=funcMissionGetActiveList, args=())
    thread.start()
    uvicorn.run(app, host='0.0.0.0')
    flagStopThread = True
    print("Stopped")






# ================================================================================================== API Command ==================================================================================================
# cl.sendto(b'{"command":"devC.deviceDic[1].optionsLoader.load(carLib.RAM.DEV.BTN_EMC)","serialNumber": 0}0000', ser)
# msg = cl.recvfrom(8192)
# output1=msg[0].decode('utf-8')
# print(output1)

# cl.sendto(b'{"command":"missionC.missionGetActiveList()","serialNumber": 1}0000', ser)
# msg = cl.recvfrom(4096)
# output3=msg[0].decode('utf-8')

# cl.sendto(b'{"command":"missionC.maGetList(1531)","serialNumber": 1}0000', ser)
# msg = cl.recvfrom(4096)
# output4=msg[0].decode('utf-8')

# cl.sendto(b'{"command":"missionC.startStation.getList(1531)","serialNumber": 1}0000', ser)
# msg = cl.recvfrom(4096)
# output5=msg[0].decode('utf-8')
#made by drum
import os
import json
import sqlite3
import argparse
import shutil
import sys

arg_parser = argparse.ArgumentParser(formatter_class=argparse.ArgumentDefaultsHelpFormatter)
arg_parser.add_argument("-i", "--input", default="analyzedAB.db", help="name of the input database file")
arg_parser.add_argument("-o", "--output", default="report.json", help="name of the output report file")
arg_parser.add_argument("-q", "--reportQuery")
arg_parser.add_argument("-r", "--reportTitle")
args = arg_parser.parse_args()

db = sqlite3.connect(args.input)
cursor = db.cursor()
query = args.reportQuery

def IsStringEmpty(strText):
    return not (strText and strText.strip())

if IsStringEmpty(query):
    query = "SELECT * FROM view_potential_duplicates WHERE view_potential_duplicates.type = 'SpriteAtlas'"
else:
    query = query.replace('\'*\'', '*')
    print("Input Query = " + query)

print("Use Query = " + query)

try:
    cursor.execute(query)
    rows = cursor.fetchall()
    db.close()
except:
    print("SQL QUERY ERROR! in " + args.input)
    db.close()
    sys.exit(-1)

#Dooray Messenger로 보낼 수 있게 JSON형식으로 Report 작성
reportDic = {}
warningDic = {}
warningDicList = []
allDicList = []
strTxt = ''

reportDic["botName"] = "Jenkins"
reportDic["botIconImage"] = "https://wiki.jenkins-ci.org/download/attachments/2916393/logo.png"
reportDic["text"] = args.reportTitle

for row in rows:
    bundles = row[5].split('\r')
    strType = row[4]

    allDic = {}
    if "mainmenuui" in bundles and "gameui" in bundles:
        if strType == 'SpriteAtlas':
            strTxt += "Atlas Name: " + row[3]
            strTxt += "\n--Using Bundles: " + ', '.join(bundles)
            strTxt += "\n\n"
        elif strType == 'Texture2D':
            strTxt += "Texture Name: " + row[3]
            strTxt += "\n--Using Bundles: " + ', '.join(bundles)
            strTxt += "\n\n"

    allDic["Name"] = row[3]
    allDic["Type"] = strType
    allDic["Bundles"] = bundles
    allDicList.append(allDic)

warningDic["title"] = "WarningAtlasList"
warningDic["titleLink"] = ""
warningDic["text"] = strTxt
warningDic["color"] = "orange"

warningDicList.append(warningDic)
reportDic["attachments"] = warningDicList

print("[[Analyzer Result]]\n", json.dumps(allDicList, indent="\t"))

outputFullFileName = os.getcwd().replace('\\', '/') + '/' + args.output
outputDirname = os.path.dirname(outputFullFileName)
outputFileName = os.path.basename(outputFullFileName)

if not os.path.exists(outputDirname):
    os.mkdir(outputDirname)

try:
    with open(outputFullFileName, 'w', encoding="utf-8") as makeFile:
        json.dump(reportDic, makeFile, ensure_ascii=False, indent="\t")
    print("Reporting File Complete.")
except:
    print("SQL QUERY ERROR! in " + args.input)
    db.close()
    sys.exit(-1)

#추후 파일 관리(삭제, 조회 등)를 위해 db파일 이동
shutil.move(args.input, outputDirname + '/' + args.input)
print("File Moving Complete.")
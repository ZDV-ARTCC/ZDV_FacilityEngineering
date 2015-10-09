import os
import shutil
import string
import http.client
from html.parser import HTMLParser

class VorNdbHTMLParser(HTMLParser):
    headerFound = False
    vorHeaderFound = False
    ndbHeaderFound = False
    vorTextareaFound = False
    ndbTextareaFound = False
    vorData = ''
    ndbData = ''
    
    def handle_starttag(self, tag, attrs):
        if tag == 'h1':
            self.headerFound = True
        else:
            self.headerFound = False

        if tag == 'textarea':
            if self.vorHeaderFound:
                self.vorTextareaFound = True
                self.vorHeaderFound = False
            elif self.ndbHeaderFound:
                self.ndbTextareaFound = True
                self.ndbHeaderFound = False
                
    def handle_data(self, data):        
        if self.vorTextareaFound:
            self.vorData = data.replace('\\n', '\n').replace('\\\'', '\'') #bytes(data, 'utf-8').decode('unicode_escape')
            self.vorTextareaFound = False
        elif self.ndbTextareaFound:
            self.ndbData = data.replace('\\n', '\n').replace('\\\'', '\'') #bytes(data, 'utf-8').decode('unicode_escape')
            self.ndbTextareaFound = False
        elif self.headerFound:
            if data == 'VORs:':
                self.vorHeaderFound = True
            elif data == 'NDBs:':
                self.ndbHeaderFound = True

class FixesHTMLParser(HTMLParser):
    headerFound = False
    fixesHeaderFound = False
    fixesTextareaFound = False
    fixesData = ''
    
    def handle_starttag(self, tag, attrs):
        if tag == 'h1':
            self.headerFound = True
        else:
            self.headerFound = False

        if tag == 'textarea':
            if self.fixesHeaderFound:
                self.fixesTextareaFound = True
                self.fixesHeaderFound = False
                
    def handle_data(self, data):        
        if self.fixesTextareaFound:
            self.fixesData = data.replace('\\n', '\n').replace('\\\'', '\'') #bytes(data, 'utf-8').decode('unicode_escape')
            self.fixesTextareaFound = False
        elif self.headerFound:
            if data == 'FIXs:':
                self.fixesHeaderFound = True

class AirwaysHTMLParser(HTMLParser):
    headerFound = False
    airwaysHeaderFound = False
    highTextareaFound = False
    lowTextareaFound = False
    highData = ''
    lowData = ''
    
    def handle_starttag(self, tag, attrs):
        if tag == 'h1':
            self.headerFound = True
        else:
            self.headerFound = False

        if tag == 'textarea':
            if self.airwaysHeaderFound:
                self.highTextareaFound = True
                self.airwaysHeaderFound = False
                
    def handle_data(self, data):        
        if self.highTextareaFound:
            self.highData = data.replace('\\n', '\n').replace('\\\'', '\'') #bytes(data, 'utf-8').decode('unicode_escape')
            self.highTextareaFound = False
            #Low textarea is immediately after High without a header
            self.lowTextareaFound=  True
        elif self.lowTextareaFound:
            self.lowData = data.replace('\\n', '\n').replace('\\\'', '\'') #bytes(data, 'utf-8').decode('unicode_escape')
            self.lowTextareaFound = False
        elif self.headerFound:
            if data == 'All Airways Formatted for VRC:':
                self.airwaysHeaderFound = True

sctFileName = 'E:\Documents\Repositories\ZDV_FacilityEngineering\VRC\ZDV_ARTCC_Sector.sct2'
sctOldFileName = 'E:\Documents\Repositories\ZDV_FacilityEngineering\VRC\ZDV_ARTCC_Sector_OLD.sct2'

urlMyFSim = 'www.myfsim.com'
urlVorNdb = '/sectorfilecreation/NavDump.php'
urlFixes = '/sectorfilecreation/FixDump.php'
urlAirways = '/sectorfilecreation/AwyDump.php'

print('Downloading data from ' + urlMyFSim)

connMyFSim = http.client.HTTPConnection(urlMyFSim)

# Request VOR/NDB page
connMyFSim.request('GET', urlVorNdb)
resVorNdb = connMyFSim.getresponse()
dataVorNdb = str(resVorNdb.read()).replace('&#39;', '\\\'')

# Request FIXES page
connMyFSim.request('GET', urlFixes)
resFixes = connMyFSim.getresponse()
dataFixes = str(resFixes.read()).replace('&#39;', '\\\'')

# Request Airways page
connMyFSim.request('GET', urlAirways)
resAirways = connMyFSim.getresponse()
dataAirways = str(resAirways.read()).replace('&$39;', '\\\'')

connMyFSim.close()

# Parse the HTML
print('Parsing HTML')

vorndbParser = VorNdbHTMLParser()
vorndbParser.feed(dataVorNdb)

fixesParser = FixesHTMLParser()
fixesParser.feed(dataFixes)

airwaysParser = AirwaysHTMLParser()
airwaysParser.feed(dataAirways)


print('Copying ' + sctFileName)

os.rename(sctFileName, sctOldFileName)
#shutil.copyfile(sctOldFileName, sctFileName)

oldFile = open(sctOldFileName, 'rt')
newFile = open(sctFileName, 'wt')

line = oldFile.readline()

# Copy each line until [VOR] is found
while line != '' and line.strip()[0:5] != '[VOR]':
  #print(line)
  newFile.write(line)  
  line = oldFile.readline()

newFile.write(line)

# Write the new VOR data
newFile.write(vorndbParser.vorData)
newFile.write('\n')
 
# Skip each line until [NDB] is found
while line != '' and line.strip()[0:5] != '[NDB]':
  line = oldFile.readline()
  
newFile.write(line)

# Write the new NDB data
newFile.write(vorndbParser.ndbData)
newFile.write('\n')

# Skip each line until [RUNWAY] section
while line != '' and line.strip()[0:32] != '; [ AIRPORT section definition ]':
  line = oldFile.readline()

# Copy each line until [FIXES] is found
while line != '' and line.strip()[0:7] != '[FIXES]':
  newFile.write(line)
  line = oldFile.readline()

newFile.write(line)

# Write the new FIXES data
newFile.write(fixesParser.fixesData)
newFile.write('\n')

# Skip each line until ; [ Airway section definition ]
while line != '' and line.strip()[0:31] != '; [ Airway section definition ]':
    line = oldFile.readline()              

# Copy each line until [HIGH AIRWAY]
while line != '' and line.strip()[0:13] != '[HIGH AIRWAY]':
    newFile.write(line)
    line = oldFile.readline()

# Write the new HIGH AIRWAY data
newFile.write(airwaysParser.highData)
newFile.write('\n')

# Skip each line until [LOW AIRWAY]
while line != '' and line.strip()[0:12] != '[LOW AIRWAY]':
    line = oldFile.readline()

# Write the new LOW AIRWAY data
newFile.write(airwaysParser.lowData)
newFile.write('\n')

# Skip each line until [STAR]
while line != '' and line.strip()[0:6] != '[STAR]':
    line = oldFile.readline()

# Copy each line until the end
while line != '':
  newFile.write(line)
  line = oldFile.readline()
  
oldFile.close()
newFile.close()

print('Completed!')



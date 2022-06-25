read -p "Er du sikker? Overskriv SERVER med lokale filer?" -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]
then
	echo Publish
	scp -r D:/SSD/C#\ projects/BossFight/BossFightBackEnd/bin/Debug/net6.0/publish pi@raspberrypi:~/bossfight
	echo Done
fi

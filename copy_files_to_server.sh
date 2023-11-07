read -p "Er du sikker? Overskriv SERVER med lokale filer?" -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]
then
	echo Publish
	echo publish folder...
	scp -r D:/SSD/C#\ projects/BossFight/BossFightSource/bin/Debug/net7.0/publish pi@raspberrypi:~/bossfight
	echo frontend...
	scp -r D:/SSD/C#\ projects/BossFight/BossFightSource/BossFightFrontEnd/* pi@raspberrypi:~/bossfight/frontend
	echo Done
fi

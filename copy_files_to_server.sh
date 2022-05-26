read -p "Er du sikker? Overskriv SERVER med lokale filer?" -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]
then
	echo Publish
	scp -r D:/SSD/C#\ projects/BossFight/BossFightBackEnd/bin/Debug/netcoreapp3.1/publish pi@raspberrypi:~/bossfight_published
	echo Done
fi

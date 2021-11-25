read -p "Er du sikker? Overskriv SERVER med lokale filer?" -n 1 -r
echo
if [[ $REPLY =~ ^[Yy]$ ]]
then
	echo Publish
	scp -r E:/C#\ projects/BossFight/BossFightBackEnd/bin/Release/netcoreapp3.1/publish pi@raspberrypi:~/bossfight_published
	echo Done
fi

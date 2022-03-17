For further infos, you can follow this page: https://docs.microsoft.com/de-de/aspnet/core/host-and-deploy/linux-nginx?view=aspnetcore-5.0

This file explains only how to setup a service quickly.

* Make a folder in "/var/dotnetwww/irmaonair_web"
* Download the application from Release tab and extract it in the folder mentioned before
* Download the "ioaweb.service" file from this folder and copy it into systemd. (sudo cp ioaweb.service /etc/systemd/system/ioaweb.service)
* Activate the systemd service (sudo systemctl enable ioaweb.service)
* Start the systemd service (sudo systemctl start ioaweb.service)
* Check if the service runs (sudo systemctl status ioaweb.service)
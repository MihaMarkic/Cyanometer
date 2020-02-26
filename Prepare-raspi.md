# Setup a new device
## Install raspbian-lite
Change password  
dist-upgrade  
enable camera (raspi-config)  
expand partition (raspi-config)

## Install OpenVPN
[Create key](https://hub.docker.com/r/kylemanna/openvpn/) with no password.

Can use `docker-compose run openvpn bash`  and then commands.

Make sure that address is vpn.rthand.com and not openvpn.rthand.com in generated .ovpn file.

Add CLIENTNAME entry ipconfig-push into OpenVPN server */etc/openVpn/ccd* directory to have a static IP.

[How to setup OpenVPN Client](https://askubuntu.com/questions/460871/how-to-setup-openvpn-client)
`apt-get install openvpn`  
Test: `openvpn --config /path/to/config.ovpn`  
Copy `.ovpn` into `/etc/openvpn` and rename it to `.conf`  
Replace `redirect-gateway def1` with `route 192.168.255.0 255.255.255.0` in .conf to avoid traffic routed through vpn.

## Configure timezone

```
dpkg-reconfigure tzdata
```

## Install Mono
Install dirmngr

`sudo apt-get install dirmngr`

[Install Mono on Raspbian](http://www.mono-project.com/download/#download-lin-raspbian)
Also install `mono-vbnc`

## Install WittyPI software if required
[Installation](http://www.uugear.com/product/witty-pi-realtime-clock-and-power-management-for-raspberry-pi/)

`wget http://www.uugear.com/repo/WittyPi/installWittyPi.sh`  
`pi@raspberrypi ~ $ sudo sh installWittyPi.sh`

## Change hostname
raspi-config
or
[How to Change Your Raspberry Pi (or Other Linux Device’s) Hostname](https://www.howtogeek.com/167195/how-to-change-your-raspberry-pi-or-other-linux-devices-hostname/)  
`sudo nano /etc/hosts`  
`sudo nano /etc/hostname`  
`sudo /etc/init.d/hostname.sh`  
`sudo reboot`  

## Install cyanometer
Copy exe and files from *src\Cyanometer\Cyanometer.Manager\bin\Release*  
Add proper `daylight.xml file`  
Add proper `applicationSettings.config`  
Copy `NLog.config` from *src/Cyanometer/Cyanometer.Manager*

Add entry to /etc/rc.local
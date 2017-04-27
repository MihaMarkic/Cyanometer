# Setup a new device
## Install raspbian-lite
Change password
dist-upgrade
enable camera (raspi-config)
expand partition (raspi-config)

## Install OpenVPN
[How to setup OpenVPN Client](https://askubuntu.com/questions/460871/how-to-setup-openvpn-client)
`apt-get install openvpn`  
Test: `openvpn --config /path/to/config.ovpn`  
Copy `.ovpn` into `/etc/openvpn` and rename it to `.conf`

## Install Mono
[Install Mono on Linux](http://www.mono-project.com/docs/getting-started/install/linux/)  
Also install `mono-vbnc`

## Change hostname
raspi-config
or
[How to Change Your Raspberry Pi (or Other Linux Device’s) Hostname](https://www.howtogeek.com/167195/how-to-change-your-raspberry-pi-or-other-linux-devices-hostname/)  
`sudo nano /etc/hosts`  
`sudo nano /etc/hostname`  
`sudo /etc/init.d/hostname.sh`  
`sudo reboot`  

## Install cyanometer
Copy exe and files  
Add proper `daylight.xml file`  
Add proper `applicationSettings.config`  

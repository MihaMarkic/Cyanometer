# Setup a new device
## Install raspbian-lite
Change password  
dist-upgrade  
enable camera (raspi-config)  
expand partition (raspi-config)

## Install OpenVPN
[Create key](https://hub.docker.com/r/kylemanna/openvpn/) with no password: 

```
easyrsa build-client-full CLIENTNAME nopas
ovpn_getclient CLIENTNAME > CLIENTNAME.ovpn
```
ca.key password is stored in Keepass under `OVPN ca`.

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

Also install Let's encyrpt root certificate as trusted.
Open https://misc.rthand.com/ certificate (in Firefox), download chain as pem to raspbery (also stored in `Data` directory) and call `sudo cert-sync cyanowatch-rthand-com-chain.pem` (perhaps more than once)

## Install WittyPI software if required
[Installation](http://www.uugear.com/product/witty-pi-realtime-clock-and-power-management-for-raspberry-pi/) (old)

`wget http://www.uugear.com/repo/WittyPi/installWittyPi.sh`  
`pi@raspberrypi ~ $ sudo sh installWittyPi.sh`

[New](https://www.uugear.com/product/witty-pi-3-mini-realtime-clock-and-power-management-for-raspberry-pi/)
```
wget http://www.uugear.com/repo/WittyPi3/install.sh
sudo sh install.sh
rm /etc/init.d/uwi
```

## Locales installation (where needed)
When using Sabra air quality service (Geneva Air), de_DE locale is required
apt clean && apt -y update && apt install -y locales && locale-gen de_DE.UTF-8 && locale -a \
	&& apt clean

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

Add entry to /etc/rc.local before last line:

`mono /home/pi/cyano/cyanometer.exe &`

## Wireguard

`sudo apt update && apt install wireguard`
Copy configuration to `/etc/wireguard/wg0.conf`
Test start with `sudo wg-quick up wg0`
`sudo systemctl enable --now wg-quick@wg0` to enable as service
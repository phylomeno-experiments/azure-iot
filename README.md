# azure-iot

## Setup Azure IoT Hub

```
az login
az group create --name iotproject --location northeurope
az iot hub create --name iotproject-phylomeno --resource-group iotproject --sku S1
```

## Query connection strings
```
az iot hub policy show --name service --query primaryKey --hub-name phylomeno-hub1
```

## Certificates
### Generate Root CA
```
openssl genrsa -des3 -out myCA.key 2048
openssl req -x509 -new -nodes -key myCA.key -sha256 -days 1825 -out myCA.pem
```

### Generate CSR 
```
openssl genrsa -out device1.key 2048
openssl req -new -key dev.deliciousbrains.com.key -out device1.csr
``` 

### Sign CSR
```
openssl x509 -req -in device1.csr -CA myCA.pem -CAkey myCA.key -CAcreateserial -out device1.crt -days 365 -sha256
```

### Generate PFX
```
openssl pkcs12 -export -out device1.pfx -inkey device1.key -in device1.crt
```

## Azure IoT Central
```
az iot central app monitor-events --app-id phylomeno --properties all
```



CREATE TABLE tags(
id_tag INT PRIMARY KEY AUTO_INCREMENT,
nombre_tag VARCHAR(20)
);

CREATE TABLE clases(
id_clase INT PRIMARY KEY AUTO_INCREMENT,
nombre_clase VARCHAR(20) NOT NULL,
costo INT NOT NULL,
duracion_dias INT NOT NULL,
cupos INT NOT NULL
);

CREATE TABLE membresias(
id_membresia INT PRIMARY KEY AUTO_INCREMENT,
nombre_membresia VARCHAR(20) NOT NULL,
precio INT NOT NULL,
duracion_dias INT NOT NULL,
descripcion VARCHAR(50) NULL,
estado ENUM("activa","inactiva","temporada", "oferta")
);

CREATE TABLE usuario_web(
id_usuario_web INT PRIMARY KEY AUTO_INCREMENT,
correo_web varchar(50) NOT NULL,
contraseña VARCHAR(60) NULL,
telefono BIGINT NOT NULL,
google_id VARCHAR(255) NULL,
google_foto VARCHAR(500) NULL,
tipo_auth ENUM("local","google") NOT NULL
);


CREATE TABLE usuarios (
id_usuario INT PRIMARY KEY AUTO_INCREMENT,
id_usuario_web INT NULL,
nombre_completo VARCHAR(70),
edad INT NOT NULL,
ciudad VARCHAR(20),
telefono BIGINT NOT NULL,
telefono_emergencia BIGINT NOT NULL,
correo VARCHAR(50) NOT NULL,
fecha_inscripcion DATETIME NOT NULL,
fecha_renovacion DATETIME,
id_membresia INT NOT NULL,
id_clase INT NULL,
estado ENUM("activo", "inactivo") NOT NULL,
id_tag INT NULL
);

CREATE TABLE roles(
id_rol INT PRIMARY KEY NOT NULL,
nombre_rol VARCHAR(20) NOT NULL
);

CREATE TABLE empleados(
id_empleado INT PRIMARY KEY AUTO_INCREMENT,
nombre_completo VARCHAR(70) NOT NULL,
correo VARCHAR(50) NOT NULL,
telefono BIGINT NOT NULL,
contraseña VARCHAR(60) NULL,
RFC VARCHAR(13),
id_rol INT NOT NULL
);

CREATE TABLE producto(
id_producto INT PRIMARY KEY AUTO_INCREMENT,
nombre_producto VARCHAR(50) NOT NULL,
precio DECIMAL NOT NULL,
stock int NOT NULL,
descripcion VARCHAR(100)
);

CREATE TABLE orden_web(
id_orden INT PRIMARY KEY AUTO_INCREMENT,
id_usuario_web INT NOT NULL,
id_producto INT NOT NULL,
cantidad INT NOT NULL,
precio DECIMAL NOT NULL,
fecha DATETIME NOT NULL,
monto_total DECIMAL NOT NULL
);

CREATE TABLE ventas(
id_venta INT PRIMARY KEY AUTO_INCREMENT,
id_usuario INT NOT NULL,
id_empleado INT NOT NULL,
fecha DATETIME NOT NULL,
monto_total DECIMAL NOT NULL,
cantidad INT NOT NULL,
tipo_venta ENUM("membresia","clase","producto")
);

CREATE TABLE biometria(
id_biometria INT PRIMARY KEY AUTO_INCREMENT,
id_usuario INT NOT NULL,
huella_digital TEXT NOT NULL,
fecha_registro DATETIME NOT NULL
);

CREATE TABLE acceso(
id_acceso INT PRIMARY KEY AUTO_INCREMENT,
id_usuario INT NOT NULL,
fecha_hora_entrada DATETIME,
fecha_hora_salida DATETIME,
metodo_verificacion ENUM("sensor biometrico","codigo QR")
);

CREATE TABLE records_personales(
id_pr INT PRIMARY KEY AUTO_INCREMENT,
id_usuario INT NOT NULL,
ejercicio ENUM("PB","PM","SL" ),
peso_kg DECIMAL NOT NULL
);




USE DB_ByleAdministration;
 
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
id_tag INT NULL,
FOREIGN KEY (id_usuario_web) REFERENCES usuario_web(id_usuario_web),
FOREIGN KEY (id_membresia) REFERENCES membresias(id_membresia),
FOREIGN KEY (id_clase) REFERENCES clases(id_clase),
FOREIGN KEY (id_tag) REFERENCES tags(id_tag)
);

CREATE TABLE roles(
id_rol INT PRIMARY KEY NOT NULL,
nombre_rol VARCHAR(20) NOT NULL
);

ALTER TABLE roles
MODIFY COLUMN id_rol INT NOT NULL AUTO_INCREMENT;

CREATE TABLE empleados(
id_empleado INT PRIMARY KEY AUTO_INCREMENT,
nombre_completo VARCHAR(70) NOT NULL,
correo VARCHAR(50) NOT NULL,
telefono BIGINT NOT NULL,
contraseña VARCHAR(60) NULL,
RFC VARCHAR(13),
id_rol INT NOT NULL,
FOREIGN KEY (id_rol) REFERENCES roles(id_rol)
);

CREATE TABLE productos(
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
monto_total DECIMAL NOT NULL,
FOREIGN KEY (id_usuario_web) REFERENCES usuario_web(id_usuario_web),
FOREIGN KEY (id_producto) REFERENCES productos(id_producto)
);

CREATE TABLE ventas(
id_venta INT PRIMARY KEY AUTO_INCREMENT,
id_usuario INT NOT NULL,
id_empleado INT NOT NULL,
fecha DATETIME NOT NULL,
monto_total DECIMAL NOT NULL,
cantidad INT NOT NULL,
tipo_venta ENUM("membresia","clase","producto"),
FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario),
FOREIGN KEY (id_empleado) REFERENCES empleados(id_empleado)
);

CREATE TABLE biometria(
id_biometria INT PRIMARY KEY AUTO_INCREMENT,
id_usuario INT NOT NULL,
huella_digital TEXT NOT NULL,
fecha_registro DATETIME NOT NULL,
FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
);

CREATE TABLE acceso(
id_acceso INT PRIMARY KEY AUTO_INCREMENT,
id_usuario INT NOT NULL,
fecha_hora_entrada DATETIME,
fecha_hora_salida DATETIME,
metodo_verificacion ENUM("sensor biometrico","codigo QR"),
FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
);

CREATE TABLE records_personales(
id_pr INT PRIMARY KEY AUTO_INCREMENT,
id_usuario INT NOT NULL,
ejercicio ENUM("PB","PM","SL" ),
peso_kg DECIMAL NOT NULL,
FOREIGN KEY (id_usuario) REFERENCES usuarios(id_usuario)
);

show create table biometria;

use DB_ByleAdministration;
SELECT * FROM roles;


INSERT INTO roles (id_rol, nombre_rol) VALUES
(2, 'Administrador');


INSERT INTO tags (id_tag, nombre_tag) VALUES
(1, 'VIP'),
(2, 'Nuevo'),
(3, 'Referido'),
(4, 'Congelado'),
(5, 'Vetado');

INSERT INTO membresias (id_membresia, nombre_membresia, precio, duracion_dias, descripcion, estado) VALUES
(1, 'Básico',     399,  30,  'Acceso ilimitado al gimnasio, horario completo.',                           'activa'),
(2, 'Estándar',   649,  30,  'Acceso completo + inscripción a clases grupales.',                          'activa'),
(3, 'Premium',    899,  30,  'Acceso total + clases grupales + asesoría nutricional personalizada.',       'activa'),
(4, 'Semestral',  3200, 180, 'Plan Estándar por 6 meses con descuento.',                                  'activa'),
(5, 'Anual',      5800, 365, 'Plan Estándar por 12 meses, máximo ahorro.',                                'activa'),
(6, 'Estudiante', 299,  30,  'Plan Básico con precio especial, requiere credencial vigente.',              'oferta'),
(7, 'Prueba',     0,    7,   'Semana de prueba gratuita para clientes nuevos.',                            'activa');

INSERT INTO clases (id_clase, nombre_clase, costo, duracion_dias, cupos) VALUES
(1, 'Zumba',       150, 30, 20),
(2, 'CrossFit',    200, 30, 15),
(3, 'Yoga',        150, 30, 18),
(4, 'Spinning',    180, 30, 12),
(5, 'Box',         220, 30, 10);





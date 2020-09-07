CREATE TABLE IF NOT EXISTS `distributedCache`
(
	`Id` varchar(767) CHARACTER SET ascii COLLATE ascii_bin NOT NULL,
	`AbsoluteExpiration` datetime(6) DEFAULT NULL,
	`ExpiresAt` datetime(6) NOT NULL,
	`SlidingExpiration` time DEFAULT NULL,
	`Value` longblob NOT NULL,
	PRIMARY KEY(`Id`),
	KEY `Index_ExpiresAt` (`ExpiresAt`)
);

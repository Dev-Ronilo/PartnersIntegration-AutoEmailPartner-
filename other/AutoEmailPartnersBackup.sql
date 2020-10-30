/*
SQLyog Community v11.51 (32 bit)
MySQL - 5.5.35 : Database - kpadminpartners
*********************************************************************
*/

/*!40101 SET NAMES utf8 */;

/*!40101 SET SQL_MODE=''*/;

/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;
CREATE DATABASE /*!32312 IF NOT EXISTS*/`kpadminpartners` /*!40100 DEFAULT CHARACTER SET latin1 */;

USE `kpadminpartners`;

/*Table structure for table `autoEmailPartners` */

DROP TABLE IF EXISTS `autoEmailPartners`;

CREATE TABLE `autoEmailPartners` (
  `emailAdd` varchar(200) NOT NULL,
  `accountName` varchar(200) DEFAULT NULL,
  `IsEmailed` int(10) NOT NULL DEFAULT '0',
  `DateEmailed` datetime DEFAULT NULL,
  `groupDept` int(10) DEFAULT NULL,
  `accountID` varchar(200) DEFAULT NULL,
  `currency` varchar(200) DEFAULT NULL,
  PRIMARY KEY (`emailAdd`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1;

/*Data for the table `autoEmailPartners` */

insert  into `autoEmailPartners`(`emailAdd`,`accountName`,`IsEmailed`,`DateEmailed`,`groupDept`,`accountID`,`currency`) values ('auto.emailpartner@gmai1.com','Admin',0,NULL,6,'Admin','PHP'),('jhoncel.cadiena@mlhuillier.com','SKYPAY',1,'2020-10-30 09:16:16',1,'MLCDP180005','PHP'),('michael.desucatan@mlhuillier.com','API NEW COMPANY 2',1,'2020-10-28 13:28:18',1,'MLCDP170189','PHP'),('sheldon.bacalso@mlhuillier.com','DEBBIE-APISPLIT',1,'2020-10-28 13:29:34',1,'MLCDP170197','PHP');

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

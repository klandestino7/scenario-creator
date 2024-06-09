-- --------------------------------------------------------
-- Servidor:                     127.0.0.1
-- Versão do servidor:           11.4.2-MariaDB - mariadb.org binary distribution
-- OS do Servidor:               Win64
-- HeidiSQL Versão:              12.7.0.6850
-- --------------------------------------------------------

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET NAMES utf8 */;
/*!50503 SET NAMES utf8mb4 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;


-- Copiando estrutura do banco de dados para fivem_server
CREATE DATABASE IF NOT EXISTS `fivem_server` /*!40100 DEFAULT CHARACTER SET latin1 COLLATE latin1_swedish_ci */;
USE `fivem_server`;

-- Copiando estrutura para tabela fivem_server.scenario
CREATE TABLE IF NOT EXISTS `scenario` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL DEFAULT '0',
  `createdAt` timestamp NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`id`)
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

-- Copiando dados para a tabela fivem_server.scenario: ~0 rows (aproximadamente)

-- Copiando estrutura para tabela fivem_server.scenario_peds
CREATE TABLE IF NOT EXISTS `scenario_peds` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `scenarioId` int(11) NOT NULL,
  `model` varchar(255) NOT NULL,
  `position` varchar(255) NOT NULL,
  `rotation` varchar(255) NOT NULL,
  `outfitVariation` int(11) DEFAULT 1,
  `isFreezed` tinyint(4) DEFAULT 0,
  `isInvincible` tinyint(4) DEFAULT 0,
  `scenarioAnim` varchar(255) DEFAULT NULL,
  `anim` varchar(255) DEFAULT NULL,
  `animDict` varchar(255) DEFAULT NULL,
  `flags` varchar(50) DEFAULT NULL,
  `relationship` varchar(255) DEFAULT NULL,
  `weaponModel` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_scenario_peds_scenario` (`scenarioId`),
  CONSTRAINT `FK_scenario_peds_scenario` FOREIGN KEY (`scenarioId`) REFERENCES `scenario` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

-- Copiando dados para a tabela fivem_server.scenario_peds: ~0 rows (aproximadamente)

-- Copiando estrutura para tabela fivem_server.scenario_props
CREATE TABLE IF NOT EXISTS `scenario_props` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `scenarioId` int(11) NOT NULL,
  `model` varchar(255) NOT NULL,
  `position` varchar(255) NOT NULL,
  `rotation` varchar(255) NOT NULL,
  `attachedToPedId` int(11) DEFAULT 0,
  `attachedMetadata` varchar(50) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `FK__scenario` (`scenarioId`),
  KEY `FK__scenario_peds` (`attachedToPedId`),
  CONSTRAINT `FK__scenario` FOREIGN KEY (`scenarioId`) REFERENCES `scenario` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK__scenario_peds` FOREIGN KEY (`attachedToPedId`) REFERENCES `scenario_peds` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

-- Copiando dados para a tabela fivem_server.scenario_props: ~0 rows (aproximadamente)

-- Copiando estrutura para tabela fivem_server.scenario_vehicles
CREATE TABLE IF NOT EXISTS `scenario_vehicles` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `scenarioId` int(11) NOT NULL DEFAULT 0,
  `model` varchar(255) NOT NULL,
  `position` varchar(255) NOT NULL,
  `rotation` varchar(255) NOT NULL,
  `props` varchar(255) NOT NULL DEFAULT '[]',
  `plate` varchar(255) DEFAULT NULL,
  `pedDriver` int(11) DEFAULT NULL,
  `driverMetadata` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`id`),
  KEY `FK_scenario_vehicles_scenario` (`scenarioId`),
  KEY `FK_scenario_vehicles_scenario_peds` (`pedDriver`),
  CONSTRAINT `FK_scenario_vehicles_scenario` FOREIGN KEY (`scenarioId`) REFERENCES `scenario` (`id`) ON DELETE CASCADE ON UPDATE CASCADE,
  CONSTRAINT `FK_scenario_vehicles_scenario_peds` FOREIGN KEY (`pedDriver`) REFERENCES `scenario_peds` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

-- Copiando dados para a tabela fivem_server.scenario_vehicles: ~0 rows (aproximadamente)

/*!40103 SET TIME_ZONE=IFNULL(@OLD_TIME_ZONE, 'system') */;
/*!40101 SET SQL_MODE=IFNULL(@OLD_SQL_MODE, '') */;
/*!40014 SET FOREIGN_KEY_CHECKS=IFNULL(@OLD_FOREIGN_KEY_CHECKS, 1) */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40111 SET SQL_NOTES=IFNULL(@OLD_SQL_NOTES, 1) */;

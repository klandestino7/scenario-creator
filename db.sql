-- Copiando estrutura do banco de dados para fivem_server
CREATE DATABASE IF NOT EXISTS `fivem_server` /*!40100 DEFAULT CHARACTER SET latin1 COLLATE latin1_swedish_ci */;
USE `fivem_server`;

-- Copiando estrutura para tabela fivem_server.scenario
CREATE TABLE IF NOT EXISTS `scenario` (
  `id` int(11) NOT NULL AUTO_INCREMENT,
  `name` varchar(255) NOT NULL DEFAULT '0',
  `defaultPosition` varchar(255) DEFAULT '{"X":0.0,"Y":0.0,"Z":0.0,"IsNormalized":false,"IsZero":false}',
  `createdAt` timestamp NOT NULL DEFAULT current_timestamp(),
  PRIMARY KEY (`id`)
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

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
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

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
  CONSTRAINT `FK__scenario` FOREIGN KEY (`scenarioId`) REFERENCES `scenario` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

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
  CONSTRAINT `FK_scenario_vehicles_scenario` FOREIGN KEY (`scenarioId`) REFERENCES `scenario` (`id`) ON DELETE CASCADE ON UPDATE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=1 DEFAULT CHARSET=latin1 COLLATE=latin1_swedish_ci;

SET FOREIGN_KEY_CHECKS=0;

-- ----------------------------
-- Table structure for `accounts`
-- ----------------------------
DROP TABLE IF EXISTS `account`;
CREATE TABLE `account` (
    `id` int(11) NOT NULL AUTO_INCREMENT,
    `password` varchar(128) NOT NULL DEFAULT '',
    `password2` varchar(128) NOT NULL DEFAULT '',
    `loggedin` tinyint(4) NOT NULL DEFAULT '0',
    `lastlogin` timestamp NULL DEFAULT NULL,
    `createdate` timestamp NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `birthday` date NOT NULL DEFAULT '0000-00-00',
    `banned` tinyint(1) NOT NULL DEFAULT '0',
    `banreason` text,
    `gm` tinyint(1) NOT NULL DEFAULT '0',
    `point` int(11) DEFAULT '0',
    PRIMARY KEY (`id`)
)  ENGINE=InnoDB AUTO_INCREMENT=10000 DEFAULT CHARSET=utf8;

-- ----------------------------
-- Table structure for `characters`
-- ----------------------------
DROP TABLE IF EXISTS `chars`;
CREATE TABLE `chars` (
    `id` int(11) NOT NULL AUTO_INCREMENT,
    `accountid` int(11) NOT NULL DEFAULT '0',
    `world` tinyint(3) NOT NULL DEFAULT '0',
    `name` varbinary(14) NOT NULL DEFAULT '\0',
    `level` tinyint(3) UNSIGNED NOT NULL DEFAULT '1',
    `exp` int(11) NOT NULL DEFAULT '6',
    `exp_tot` int(11) NOT NULL DEFAULT '6',
    `hp` int(11) NOT NULL DEFAULT '81',
    `sp` int(11) NOT NULL DEFAULT '61',
    `mag` int(11) NOT NULL DEFAULT '0',
    `atk` int(11) NOT NULL DEFAULT '0',
    `def` int(11) NOT NULL DEFAULT '0',
    `hpx` int(11) NOT NULL DEFAULT '0',
    `spx` int(11) NOT NULL DEFAULT '0',
    `agi` int(11) NOT NULL DEFAULT '0',
    `mag2` int(11) NOT NULL DEFAULT '0',
    `atk2` int(11) NOT NULL DEFAULT '0',
    `def2` int(11) NOT NULL DEFAULT '0',
    `hp2` int(11) NOT NULL DEFAULT '0',
    `sp2` int(11) NOT NULL DEFAULT '0',
    `agi2` int(11) NOT NULL DEFAULT '0',
    `sk_point` int(11) NOT NULL DEFAULT '0',
    `stt_point` int(11) NOT NULL DEFAULT '0',
    `sex` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
    `ghost` tinyint(3) UNSIGNED NOT NULL DEFAULT '0',
    `god` tinyint(3) NOT NULL DEFAULT '0',
    `style` tinyint(1) NOT NULL DEFAULT '0',
    `hair` tinyint(1) NOT NULL DEFAULT '0',
    `face` tinyint(1) NOT NULL DEFAULT '0',
    `color1` int(11) NOT NULL DEFAULT '0',
    `color2` int(11) NOT NULL DEFAULT '0',
    `map_id` smallint(5) UNSIGNED NOT NULL DEFAULT '12001',
    `map_x` smallint(5) UNSIGNED NOT NULL DEFAULT '1000',
    `map_y` smallint(5) UNSIGNED NOT NULL DEFAULT '1000',
    `gold` int(11) NOT NULL DEFAULT '0',
    `gold_bank` int(11) NOT NULL DEFAULT '0',
    `element` tinyint(1) NOT NULL DEFAULT '0',
    `reborn` tinyint(1) NOT NULL DEFAULT '0',
    `job` tinyint(1) NOT NULL DEFAULT '0',
    `honor` int(11) NOT NULL DEFAULT '0',
    `pet_battle` tinyint(1) NOT NULL DEFAULT '-1',
	`equip` varbinary(100) NOT NULL DEFAULT '\0',
	`inventory` varbinary(300) NOT NULL DEFAULT '\0',
	`bag` varbinary(300) NOT NULL DEFAULT '\0',
	`storage` varbinary(600) NOT NULL DEFAULT '\0',
	`skill` varbinary(600) NOT NULL DEFAULT '\0',
	`hotkey` varbinary(30) NOT NULL DEFAULT '\0',
    PRIMARY KEY (`id`),
    KEY `accountid` (`accountid`)
)  ENGINE=InnoDB AUTO_INCREMENT=10000 DEFAULT CHARSET=utf8;

DROP TABLE IF EXISTS `pet`;
CREATE TABLE `pet` (
    `pet_sid` int(10) unsigned NOT NULL AUTO_INCREMENT,
    `charid` int(11) NOT NULL,
    `npcid` smallint(11) UNSIGNED NOT NULL DEFAULT '0',
    `name` varbinary(14) NOT NULL DEFAULT '\0',
    `level` tinyint(11) UNSIGNED NOT NULL DEFAULT '1',
    `exp` int(11) NOT NULL DEFAULT '6',
    `exp_tot` int(11) NOT NULL DEFAULT '6',
    `hp` int(11) NOT NULL DEFAULT '1',
    `sp` int(11) NOT NULL DEFAULT '1',
    `mag` int(11) NOT NULL DEFAULT '0',
    `atk` int(11) NOT NULL DEFAULT '0',
    `def` int(11) NOT NULL DEFAULT '0',
    `hpx` int(11) NOT NULL DEFAULT '0',
    `spx` int(11) NOT NULL DEFAULT '0',
    `agi` int(11) NOT NULL DEFAULT '0',
    `mag2` int(11) NOT NULL DEFAULT '0',
    `atk2` int(11) NOT NULL DEFAULT '0',
    `def2` int(11) NOT NULL DEFAULT '0',
    `hp2` int(11) NOT NULL DEFAULT '0',
    `sp2` int(11) NOT NULL DEFAULT '0',
    `agi2` int(11) NOT NULL DEFAULT '0',
    `reborn` tinyint(11) NOT NULL DEFAULT '0',
    `sk_point` smallint(11) NOT NULL DEFAULT '0',
    `fai` tinyint(11) UNSIGNED NOT NULL DEFAULT '60',
    `slot` tinyint(11) NOT NULL DEFAULT '0',
    `location` tinyint(11) NOT NULL DEFAULT '0',
    `sk1_lvl` tinyint(11) NOT NULL DEFAULT '1',
    `sk2_lvl` tinyint(11) NOT NULL DEFAULT '1',
    `sk3_lvl` tinyint(11) NOT NULL DEFAULT '1',
    `sk4_lvl` tinyint(11) NOT NULL DEFAULT '0',
	`equip` varbinary(100) NOT NULL DEFAULT '\0',
    PRIMARY KEY (`pet_sid`),
    KEY `pet_charid` (`charid`),
    CONSTRAINT `pet_destroy` FOREIGN KEY (`charid`)
        REFERENCES `chars` (`id`)
        ON DELETE CASCADE
)  ENGINE=InnoDB AUTO_INCREMENT=10000 DEFAULT CHARSET=utf8;

 insert into account (password, password2) values ('aaaaaa','qqqqqq');
 insert into account (password, password2) values ('aaaaaa','qqqqqq');
 insert into account (password, password2) values ('aaaaaa','qqqqqq');
 insert into account (password, password2) values ('aaaaaa','qqqqqq');
 insert into account (password, password2) values ('aaaaaa','qqqqqq');
 insert into account (password, password2) values ('aaaaaa','qqqqqq');
 insert into account (password, password2) values ('aaaaaa','qqqqqq');
 insert into account (password, password2) values ('aaaaaa','qqqqqq');
 insert into account (password, password2) values ('aaaaaa','qqqqqq');
 insert into account (password, password2) values ('aaaaaa','qqqqqq');

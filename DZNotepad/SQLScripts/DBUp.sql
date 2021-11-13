-- Скрипт развёртки БД
-- Версия: 1.0

CREATE TABLE IF NOT EXISTS stylesNames (
    styleNameId INT NOT NULL,
    styleName NVARCHAR(45) NOT NULL,
    PRIMARY KEY (styleNameId)
);

CREATE TABLE IF NOT EXISTS styles (
    styleNameId INT NOT NULL,
    paramName NVARCHAR(45) NOT NULL,
    paramValue NVARCHAR(256) NOT NULL,
    PRIMARY KEY (styleNameId, paramName),
    FOREIGN KEY (styleNameId) REFERENCES stylesNames(styleNameId) ON DELETE CASCADE
);

-- TODO: сделать вставку начальных стилей (ситуация удаления БД)

CREATE TABLE IF NOT EXISTS lastFiles (
    filename NVARCHAR(256) NOT NULL,
    PRIMARY KEY (filename)
);

CREATE TABLE IF NOT EXISTS supportLanguages (
    languageName NVARCHAR(45) NOT NULL,
    PRIMARY KEY (languageName)
);

INSERT INTO supportLanguages VALUES ('afrikaans');
INSERT INTO supportLanguages VALUES ('albanian');
INSERT INTO supportLanguages VALUES ('amharic');
INSERT INTO supportLanguages VALUES ('arabic');
INSERT INTO supportLanguages VALUES ('armenian');
INSERT INTO supportLanguages VALUES ('azerbaijani');
INSERT INTO supportLanguages VALUES ('basque');
INSERT INTO supportLanguages VALUES ('belarusian');
INSERT INTO supportLanguages VALUES ('bengali');
INSERT INTO supportLanguages VALUES ('bosnian');
INSERT INTO supportLanguages VALUES ('bulgarian');
INSERT INTO supportLanguages VALUES ('catalan');
INSERT INTO supportLanguages VALUES ('chichewa');
INSERT INTO supportLanguages VALUES ('corsican');
INSERT INTO supportLanguages VALUES ('croatian');
INSERT INTO supportLanguages VALUES ('czech');
INSERT INTO supportLanguages VALUES ('danish');
INSERT INTO supportLanguages VALUES ('dutch');
INSERT INTO supportLanguages VALUES ('english');
INSERT INTO supportLanguages VALUES ('esperanto');
INSERT INTO supportLanguages VALUES ('estonian');
INSERT INTO supportLanguages VALUES ('filipino');
INSERT INTO supportLanguages VALUES ('finnish');
INSERT INTO supportLanguages VALUES ('french');
INSERT INTO supportLanguages VALUES ('frisian');
INSERT INTO supportLanguages VALUES ('galician');
INSERT INTO supportLanguages VALUES ('georgian');
INSERT INTO supportLanguages VALUES ('german');
INSERT INTO supportLanguages VALUES ('greek');
INSERT INTO supportLanguages VALUES ('gujarati');
INSERT INTO supportLanguages VALUES ('haitian');
INSERT INTO supportLanguages VALUES ('hausa');
INSERT INTO supportLanguages VALUES ('hebrew');
INSERT INTO supportLanguages VALUES ('hindi');
INSERT INTO supportLanguages VALUES ('chinese (simplified)');
INSERT INTO supportLanguages VALUES ('chinese (traditional)');
INSERT INTO supportLanguages VALUES ('cebuano');
INSERT INTO supportLanguages VALUES ('hawaiian');
INSERT INTO supportLanguages VALUES ('hmong');
INSERT INTO supportLanguages VALUES ('hungarian');
INSERT INTO supportLanguages VALUES ('icelandic');
INSERT INTO supportLanguages VALUES ('igbo');
INSERT INTO supportLanguages VALUES ('indonesian');
INSERT INTO supportLanguages VALUES ('irish');
INSERT INTO supportLanguages VALUES ('italian');
INSERT INTO supportLanguages VALUES ('japanese');
INSERT INTO supportLanguages VALUES ('javanese');
INSERT INTO supportLanguages VALUES ('kannada');
INSERT INTO supportLanguages VALUES ('kazakh');
INSERT INTO supportLanguages VALUES ('khmer');
INSERT INTO supportLanguages VALUES ('korean');
INSERT INTO supportLanguages VALUES ('kurdish');
INSERT INTO supportLanguages VALUES ('kyrgyz');
INSERT INTO supportLanguages VALUES ('lao');
INSERT INTO supportLanguages VALUES ('latin');
INSERT INTO supportLanguages VALUES ('latvian');
INSERT INTO supportLanguages VALUES ('lithuanian');
INSERT INTO supportLanguages VALUES ('luxembourgish');
INSERT INTO supportLanguages VALUES ('macedonian');
INSERT INTO supportLanguages VALUES ('malagasy');
INSERT INTO supportLanguages VALUES ('malay');
INSERT INTO supportLanguages VALUES ('malayalam');
INSERT INTO supportLanguages VALUES ('maltese');
INSERT INTO supportLanguages VALUES ('maori');
INSERT INTO supportLanguages VALUES ('marathi');
INSERT INTO supportLanguages VALUES ('mongolian');
INSERT INTO supportLanguages VALUES ('myanmar');
INSERT INTO supportLanguages VALUES ('nepali');
INSERT INTO supportLanguages VALUES ('norwegian');
INSERT INTO supportLanguages VALUES ('odia');
INSERT INTO supportLanguages VALUES ('pashto');
INSERT INTO supportLanguages VALUES ('persian');
INSERT INTO supportLanguages VALUES ('polish');
INSERT INTO supportLanguages VALUES ('portuguese');
INSERT INTO supportLanguages VALUES ('punjabi');
INSERT INTO supportLanguages VALUES ('romanian');
INSERT INTO supportLanguages VALUES ('russian');
INSERT INTO supportLanguages VALUES ('samoan');
INSERT INTO supportLanguages VALUES ('scots gaelic');
INSERT INTO supportLanguages VALUES ('serbian');
INSERT INTO supportLanguages VALUES ('sesotho');
INSERT INTO supportLanguages VALUES ('shona');
INSERT INTO supportLanguages VALUES ('sindhi');
INSERT INTO supportLanguages VALUES ('sinhala');
INSERT INTO supportLanguages VALUES ('slovak');
INSERT INTO supportLanguages VALUES ('slovenian');
INSERT INTO supportLanguages VALUES ('somali');
INSERT INTO supportLanguages VALUES ('spanish');
INSERT INTO supportLanguages VALUES ('sundanese');
INSERT INTO supportLanguages VALUES ('swahili');
INSERT INTO supportLanguages VALUES ('swedish');
INSERT INTO supportLanguages VALUES ('tajik');
INSERT INTO supportLanguages VALUES ('tamil');
INSERT INTO supportLanguages VALUES ('telugu');
INSERT INTO supportLanguages VALUES ('thai');
INSERT INTO supportLanguages VALUES ('turkish');
INSERT INTO supportLanguages VALUES ('ukrainian');
INSERT INTO supportLanguages VALUES ('urdu');
INSERT INTO supportLanguages VALUES ('uyghur');
INSERT INTO supportLanguages VALUES ('uzbek');
INSERT INTO supportLanguages VALUES ('vietnamese');
INSERT INTO supportLanguages VALUES ('welsh');
INSERT INTO supportLanguages VALUES ('xhosa');
INSERT INTO supportLanguages VALUES ('yiddish');
INSERT INTO supportLanguages VALUES ('yoruba');
INSERT INTO supportLanguages VALUES ('zulu');
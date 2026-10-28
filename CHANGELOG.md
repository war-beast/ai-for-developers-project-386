# Changelog

## [1.2.0](https://github.com/war-beast/ai-for-developers-project-386/compare/callcalendar-v1.1.0...callcalendar-v1.2.0) (2026-10-28)


### Features

* доработать вёрстку главной, админки и записи ([24896e8](https://github.com/war-beast/ai-for-developers-project-386/commit/24896e8aef151c8d4888370f3b037d0e02fa5f1c))
* доработать вёрстку главной, админки и записи ([#36](https://github.com/war-beast/ai-for-developers-project-386/issues/36)) ([e04d44d](https://github.com/war-beast/ai-for-developers-project-386/commit/e04d44d5eb9c572887114f08e0151792d2b2c82a))
* показывать длительность встречи в списке встреч ([#36](https://github.com/war-beast/ai-for-developers-project-386/issues/36)) ([af5deee](https://github.com/war-beast/ai-for-developers-project-386/commit/af5deeec962a3080d5e08ed34eddadcc1467fa4b))
* показывать форму записи под календарём ([#36](https://github.com/war-beast/ai-for-developers-project-386/issues/36)) ([f142223](https://github.com/war-beast/ai-for-developers-project-386/commit/f1422230bb7263140a3b443bd4349128949a8d74))


### Bug Fixes

* прокручивать список свободных слотов ([#36](https://github.com/war-beast/ai-for-developers-project-386/issues/36)) ([3b77a22](https://github.com/war-beast/ai-for-developers-project-386/commit/3b77a22a2394f9aa653455e4cfe6b4bd6cd34d8c))

## [1.1.0](https://github.com/war-beast/ai-for-developers-project-386/compare/callcalendar-v1.0.0...callcalendar-v1.1.0) (2026-10-27)


### Features

* бронировать слот из формы гостя ([10e5c11](https://github.com/war-beast/ai-for-developers-project-386/commit/10e5c115f9fc1dc9f3daef91a632ebebd5b512d2))
* добавить каталог и админку типов встреч ([2e8b212](https://github.com/war-beast/ai-for-developers-project-386/commit/2e8b2129e921cee4a7a14741570d9a3cfae9cdd1))
* добавить каталог типов встреч в DAL и BLL ([7cf1940](https://github.com/war-beast/ai-for-developers-project-386/commit/7cf1940165c477cb3402ce97bf1b12fbb5d2ae77))
* контракт записи на звонок в TypeSpec ([a2ff922](https://github.com/war-beast/ai-for-developers-project-386/commit/a2ff9221a97af057a26731680aeaeae611e5dc98))
* описать контракт записи на звонок в TypeSpec ([a4b40a9](https://github.com/war-beast/ai-for-developers-project-386/commit/a4b40a943645397a530f4988417c4b1cda8f9fce))
* отдавать предстоящие встречи владельца ([796d7b1](https://github.com/war-beast/ai-for-developers-project-386/commit/796d7b139b47b36fe6061c7b03886d5d292478d5))
* отдавать свободные слоты типа встречи на 14 дней ([ed11719](https://github.com/war-beast/ai-for-developers-project-386/commit/ed11719878b8653e5e31d688f2c68cfff93b844d))
* отклонять пересекающиеся записи в памяти ([0bbc26e](https://github.com/war-beast/ai-for-developers-project-386/commit/0bbc26e7c56b93ad4727109fa2dcef9978fafa02))
* открыть API типов встреч ([4bf616c](https://github.com/war-beast/ai-for-developers-project-386/commit/4bf616c0eed8dccd954d1cb5df0823bfa0dce493))
* показать свободные слоты на странице типа встречи ([6b1d026](https://github.com/war-beast/ai-for-developers-project-386/commit/6b1d0262d44de2153f0cbdab7bea3286a3e0fa9f))
* показывать предстоящие встречи в админке ([5e56150](https://github.com/war-beast/ai-for-developers-project-386/commit/5e561508ab846c0e31338ac30cc53bde6c8935bf))
* реализовать запись на звонок ([80def19](https://github.com/war-beast/ai-for-developers-project-386/commit/80def192ea1199089e6f5994f355b3fceb431331))
* создавать запись на свободный слот ([05638af](https://github.com/war-beast/ai-for-developers-project-386/commit/05638af13ee7beaf96e696345ef185f330bde142))
* хранить записи в памяти и искать тип по id ([fee31dc](https://github.com/war-beast/ai-for-developers-project-386/commit/fee31dc88f55a472caa8cc086c74b11c0e980952))


### Bug Fixes

* отдавать validation_failed для невалидного тела запроса ([24a8821](https://github.com/war-beast/ai-for-developers-project-386/commit/24a8821b832b93a5647ba1f3f4bd896090fba0af))
* снимать занятый слот с выбора и показывать «3 ноя, 10:00» ([5a70661](https://github.com/war-beast/ai-for-developers-project-386/commit/5a70661961e4306b3c797b301929bbc942ee74ba))
* считать предстоящие встречи без N+1 и без лимита имени гостя ([8e5bd67](https://github.com/war-beast/ai-for-developers-project-386/commit/8e5bd672deb776a9dd1e07b0b68eff440f864dd6))
* устранить SIGPIPE в smoke-скрипте ([fbd1df4](https://github.com/war-beast/ai-for-developers-project-386/commit/fbd1df45988008594c8451e02f17f08ac104e510))
* устранить SIGPIPE в smoke-скрипте ([1021367](https://github.com/war-beast/ai-for-developers-project-386/commit/1021367a8ac042a6a117258f30aa015b1f48d24f))

## 1.0.0 (2026-10-27)


### Features

* scaffold CallCalendar full-stack carcass ([722ed44](https://github.com/war-beast/ai-for-developers-project-386/commit/722ed44c62df2d5304422f8ecb17fab77e1388b2))
* главная страница и заглушка страницы записи ([754099e](https://github.com/war-beast/ai-for-developers-project-386/commit/754099e1e5ad3088446e8c49cfafa989e4649216))
* главная страница и заглушка страницы записи ([0c2262f](https://github.com/war-beast/ai-for-developers-project-386/commit/0c2262ffbeda834dde3b2c8460893170bfbfc9a1))
* липкая панель навигации и оформление в духе Metro ([6f13525](https://github.com/war-beast/ai-for-developers-project-386/commit/6f135250503e82fc53eb89c5dc210a59ab3e27cb))
* правила веток и переименование src в backend ([d4ab9a6](https://github.com/war-beast/ai-for-developers-project-386/commit/d4ab9a63605b8d5a8b766bf05d153d05019b6371))


### Bug Fixes

* bind vite daemon to 127.0.0.1 ([102f210](https://github.com/war-beast/ai-for-developers-project-386/commit/102f2105cdba4eab4c307cce2c6e81f5fb3bc163))
* use POSIX shell for typos builtin on Windows hooks ([1968ff4](https://github.com/war-beast/ai-for-developers-project-386/commit/1968ff4d009c933ac1f08b9043ade8641fe9d1c5))
* держать папку wwwroot, чтобы статический манифест не падал ([42b056c](https://github.com/war-beast/ai-for-developers-project-386/commit/42b056caa3491a8dab1818dd06b3dbf01b3462ac))
* папка wwwroot, чтобы статический манифест не падал ([405e47a](https://github.com/war-beast/ai-for-developers-project-386/commit/405e47a4459f6e24f9974341b93863d2e925eefa))

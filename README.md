Штрих коды для скачивания кидать в файл barcodes
Файлы сохраняются в папке storage - не очищается при повторном запуске. Рекомендуется очищать, иначе файлы перезаписываются
Уже скачанные коды записываются в файл DownloadBarcodes. Нужно для продолжения процесса скачивания при повторном запуске. !Рекомендуется очищать при новом запуске. Т.к. эти записи попадают в игнор
Если возникла ошибка при скачивании, такие баркоды попадают в файл NotDownloadBarcodes (просто для информативности)
Логи в папке logs

С начала пробегается по всем штрих-кодам и получает их пути расположения в ЭА. Тем самым прверяя их наличие в ЭА. 
Все провереные пути кешируются в файле fileroots.txt - очистить, если нужно актуализировать данные

Update 25.01.22:
В конфиг файле можно натраивать размеры порции проверки путей в ЭА и наименования папок.
Все проверенные пути пишутся в fileroots.txt а значения штрих-кодов в FoundedBarcodesInEa.txt
Проверенные штрих-коды на наличие в ЭА повторно не проверяются
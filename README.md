<h1>Запуск базы данных</h1>
<p>Windows: <code>Avanpost\docker\run.cmd</code></p>
<p>Linux: <code>"docker compose -f Avanpost/docker/docker-compose.yml up"</code></p>

<h1>Неясность</h1>
<p><code>IEnumerable<Property> GetAllProperties();</code> // Получить все свойства, которые можно получить для пользователя, пароль тоже свойство</p>
<p>Данное требование ( пароль тоже свойство ) <b><i>не реализовано</i></b>, т.к. в тестах захардкожено <code>DefaultData.PropsCount = 5</code>, а количество аттрибутов, исключая аттрибут <code>login</code>, у отношения <code>User</code> равно пяти.</p>

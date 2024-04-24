# Solución de prueba técnica

Hola soy Sebastián Gomez Quintero y a continuación verás la solución a la prueba técnica

# Responde la pregunta 1
La respuesta a este punto se encuentra a continuación en sql. La parte EF se puede ver durante la ejecución del proyecto iniciando la API o ejecutando la prueba unitaria.

## Modelo propuesto para los datos.

    CREATE TABLE Car (
        CarId INT PRIMARY KEY IDENTITY(1,1),
        Year INT,
        Make NVARCHAR(100),
        Model NVARCHAR(100),
        Submodel NVARCHAR(100),
        ZipCode NVARCHAR(10)
    );
    CREATE TABLE Buyer (
        BuyerId INT PRIMARY KEY IDENTITY(1,1),
        Name NVARCHAR(100)
    );
    CREATE TABLE Quote (
        QuoteId INT PRIMARY KEY IDENTITY(1,1),
        CarId INT,
        BuyerId INT,
        Amount DECIMAL(10, 2),
        IsCurrent BIT,
        FOREIGN KEY (CarId) REFERENCES Car(CarId),
        FOREIGN KEY (BuyerId) REFERENCES Buyer(BuyerId)
    );   
    CREATE TABLE Status (
        StatusId INT PRIMARY KEY IDENTITY(1,1),
        CarId INT,
        StatusName NVARCHAR(100),
        StatusDate DATETIME,
        ChangedBy NVARCHAR(100),
        FOREIGN KEY (CarId) REFERENCES Car(CarId)
    );


## Consulta para mostrar la información

    WITH LatestStatus AS (SELECT CarId, MAX(StatusId) AS MaxStatusId  
                      FROM Status  
	 GROUP BY CarId)  
	SELECT c.Year,  
	  c.Make,  
	  c.Model,  
	  c.Submodel,  
	  b.Name AS BuyerName,  
	 q.Amount AS QuoteAmount,  
	 s.StatusName,  
	 s.StatusDate  
	FROM Car c  
	         INNER JOIN Quote q ON c.CarId = q.CarId AND q.IsCurrent = 1  
	  INNER JOIN Buyer b ON q.BuyerId = b.BuyerId  
	  INNER JOIN Status s ON c.CarId = s.CarId  
	  INNER JOIN LatestStatus ls ON s.CarId = ls.CarId AND s.StatusId = ls.MaxStatusId;
## Consulta de Entity Framework

    public async Task<IEnumerable<CarInfoDto>> GetCarInformationAsync()
	{
      var cars = await _context.Cars.ToListAsync();

      var currentQuotes = await _context.Quotes
          .Where(q => q.IsCurrent)
          .Select(q => new { q.CarId, q.Buyer.Name, q.Amount })
          .ToListAsync();

      var latestStatuses = await _context.Statuses
         .GroupBy(s => s.CarId)
         .Select(g => new { g.Key, Status = g.OrderByDescending(s => s.StatusId).FirstOrDefault() })
         .ToListAsync();

      var result = cars
            .Select(c => new CarInfoDto
            {
                Year = c.Year,
                Make = c.Make,
                Model = c.Model,
                Submodel = c.Submodel,
                BuyerName = currentQuotes.FirstOrDefault(q => q.CarId == c.CarId).Name,
                QuoteAmount = currentQuotes.FirstOrDefault(q => q.CarId == c.CarId).Amount,
                StatusName = latestStatuses.FirstOrDefault(ls => ls.Key == c.CarId).Status.StatusName,
                StatusDate = latestStatuses.FirstOrDefault(ls => ls.Key == c.CarId).Status.StatusDate
            });

      return result;
     }

# Responde la pregunta 2

¿Qué harías si tuvieras datos que no cambian con frecuencia pero que se usan prácticamente todo el tiempo?

Para datos que no cambian con frecuencia pero a se accede con frecuencia, implementar una estrategia de almacenamiento en caché suele ser el mejor enfoque. El almacenamiento en caché implica almacenar datos en un almacén de datos de acceso rápido después de la primera recuperación para que las solicitudes futuras de datos puedan atenderse rápidamente, sin la necesidad de realizar consultas repetidas a la base de datos.

Hay varias estrategias de almacenamiento en caché a considerar:

1. **Caché en Memoria**: Almacenar datos directamente en la memoria de la aplicación puede acelerar significativamente el acceso. Se pueden utilizar tecnologías como Redis, Memcached o incluso caché en proceso con una herramienta como MemoryCache en .NET.
2. **Caché Distribuido**: Para aplicaciones que se escalan horizontalmente a través de múltiples servidores, un caché distribuido asegura que los datos en caché estén disponibles para todas las instancias de la aplicación. Este enfoque es beneficioso para aplicaciones nativas de la nube que requieren alta disponibilidad y escalabilidad.
3. **Caché Persistente**: A veces, los datos pueden necesitar ser almacenados en caché por períodos más largos y deberían persistir más allá de los reinicios de la aplicación. En tales casos, usar un almacén persistente como un caché basado en archivos o una base de datos puede ser adecuado.
4. **Invalidación de Caché**: Es crucial tener una estrategia para invalidar el caché cuando los datos finalmente cambian. Esto se puede hacer mediante políticas de expiración, etiquetas de versión o actualizaciones de caché impulsadas por eventos.
5. **Caché CDN**: Para aplicaciones web, datos estáticos como imágenes, CSS y archivos JavaScript se pueden almacenar en caché en una Red de Distribución de Contenidos (CDN) para mejorar los tiempos de carga para usuarios en diferentes ubicaciones geográficas.

El almacenamiento en caché puede reducir la carga de la base de datos, disminuir la latencia y mejorar la capacidad de respuesta general de una aplicación.

# Responde la pregunta 3

## Refactorización del método UpdateCustomersBalanceByInvoices

### Original 

	public void UpdateCustomersBalanceByInvoices(List<Invoice> invoices)
	{
	    foreach (var invoice in invoices)
	    {
	        var customer = dbContext.Customers.SingleOrDefault(c => c.Id == invoice.CustomerId);
	        customer.Balance -= invoice.Amount;
	        dbContext.SaveChanges();
	    }
	}

Este método tiene varios problemas que podrían optimizarse:

1. **Rendimiento**: El método llama a `dbContext.SaveChanges()` dentro de un bucle, lo que da como resultado una transacción de base de datos separada para cada factura. Esto es altamente ineficiente.
    
2. **Manejo de errores**: No hay manejo de errores para garantizar que se encuentre al "cliente" antes de intentar actualizar el saldo.
    
3. **Múltiples llamadas a la base de datos**: el método consiste en recuperar al cliente de la base de datos uno por uno en un bucle, lo que puede provocar problemas de rendimiento debido a la cantidad de llamadas a la base de datos.
    
4. **Sin manejo de concurrencia**: si la aplicación se utiliza en un entorno de subprocesos múltiples, la falta de manejo de concurrencia podría generar datos inconsistentes.

	
	    public void UpdateCustomersBalanceByInvoices(List<Invoice> invoices)
	    {
	    var customerIds = invoices.Select(i => i.CustomerId).Distinct().ToList();
	    var customers = dbContext.Customers
	                             .Where(c => customerIds.Contains(c.Id))
	                             .ToList();
	    
	    foreach (var invoice in invoices)
	    {
	        var customer = customers.SingleOrDefault(c => c.Id == invoice.CustomerId);
	        if (customer != null)
	        {
	            customer.Balance -= invoice.Amount;
	        }
	        else
	        {
	            // Handle the case where the customer is not found
	            // This could be logging the error or throwing an exception
	        }
	    }

	    dbContext.SaveChanges();
        }

En el método mejorado:

- El contexto de la base de datos `SaveChanges` se llama una vez después de todas las actualizaciones, lo que reduce la cantidad de transacciones y mejora el rendimiento.
- Los clientes se recuperan en una única consulta, lo que reduce los viajes de ida y vuelta a la base de datos.
- Se agrega la verificación de existencia de clientes para evitar excepciones de referencia nula.
- Prepara el método para una posible optimización de la actualización por lotes y un mejor manejo de errores.

# Responde la pregunta 4

## Implementación del método GetOrders

	public async Task<List<OrderDTO>> GetOrders(DateTime? dateFrom, DateTime? dateTo, List<int>? customerIds, List<int>? statusIds, bool? isActive)
	{
    // Start with a queryable that includes any necessary joins
    var query = _context.Orders.AsQueryable();

    // Apply filtering for dateFrom if provided
    if (dateFrom.HasValue)
    {
        query = query.Where(order => order.Date >= dateFrom.Value);
    }

    // Apply filtering for dateTo if provided
    if (dateTo.HasValue)
    {
        query = query.Where(order => order.Date <= dateTo.Value);
    }

    // Apply customer ID filtering if any customer IDs are provided
    if (customerIds != null && customerIds.Count > 0)
    {
        query = query.Where(order => customerIds.Contains(order.CustomerId));
    }

    // Apply status ID filtering if any status IDs are provided
    if (statusIds != null && statusIds.Count > 0)
    {
        query = query.Where(order => statusIds.Contains(order.StatusId));
    }

    // Apply active status filtering if an isActive filter is provided
    if (isActive.HasValue)
    {
        query = query.Where(order => order.IsActive == isActive.Value);
    }

    // Project the results to the DTO
    var orders = await query.Select(order => new OrderDTO
    {
        OrderId = order.Id,
        CustomerId = order.CustomerId,
        StatusId = order.StatusId,
        IsActive = order.IsActive,
        // Additional properties may be added here
    }).ToListAsync();

    return orders;
}

Responde la pregunta 5

Bill, del Departamento de Control de Calidad, le asignó una tarea de alta prioridad que indica que hay un error cuando alguien cambia el estado de "Aceptado" a "Recogido". Define cómo procederías, paso a paso, hasta crear el Pull Request.

### Enfoque paso a paso para resolver el error
1. **Reproduzca el problema**:
    
- Comience intentando reproducir el error en un entorno de desarrollo controlado. Utilice los pasos exactos proporcionados por el equipo de control de calidad o los pasos que provocaron el error informado.
- Verificar que el error ocurra consistentemente con los pasos descritos y en condiciones similares.
2. **Identifique el error**:
    
     - Examinar el código responsable de cambiar el estado de "Aceptado" a "Recogido". Es probable que esto involucre controladores de eventos o servicios que gestionan transiciones de estado.
     - Verifique si existen condiciones, validaciones o errores en la lógica que puedan provocar el problema informado.
3. **Depuración**:
    
     - Utilice herramientas de depuración para recorrer el código y monitorear el estado de la aplicación a medida que pasa de "Aceptada" a "Recogida".
     - Preste especial atención a cualquier modificación que se realice en la base de datos o en el estado de la aplicación durante esta transición.
4. **Escribe una prueba**:
    
     - Escribe una prueba unitaria que falle debido al error. Esto no sólo confirma la presencia del error sino que también garantiza que una vez solucionado, el error no vuelva a ocurrir.
     - La prueba debe imitar el cambio de estado y comprobar el resultado esperado.
5. **Implemente la solución**:
    
     - Una vez identificada la fuente del error, modifique el código para rectificar el problema. Esto podría implicar corregir la lógica de la transición de estado, manejar las excepciones de manera más adecuada o ajustar la lógica empresarial relacionada.
     - Asegúrese de que los cambios no interrumpan ninguna funcionalidad existente mediante la ejecución de pruebas existentes.
6. **Ejecutar pruebas**:
    
     - Ejecute la prueba recién creada junto con otras pruebas relevantes para garantizar que la solución funcione como se esperaba y no introduzca nuevos problemas.
     - Asegúrese de que todas las pruebas pasen antes de continuar con los siguientes pasos.
7. **Revisión de código**:
    
     - Crear una solicitud de extracción (PR) para los cambios. Proporcione una descripción clara de los cambios y el motivo de los mismos en el PR.
     - Solicite una revisión del código a sus pares. Asegúrese de que al menos otro desarrollador revise los cambios para garantizar la calidad y la mantenibilidad.
8. **Implementar los cambios**:
    
     - Una vez aprobado, fusionar el PR en la rama principal de desarrollo.
     - Siga los procedimientos estándar de su equipo para implementar cambios en el entorno de producción, lo que podría implicar etapas adicionales de prueba.
9. **Verifique la solución en producción**:
    
     - Una vez implementada, verifique que la solución resuelva el problema en el entorno de producción sin causar efectos secundarios inesperados.
10. **Documente la resolución**:

      - Actualizar cualquier documentación que refleje los cambios realizados. Esto podría incluir documentación técnica, manuales de usuario o notas de la versión.
      - La documentación ayuda a los futuros desarrolladores a comprender los cambios y las razones detrás de ellos, lo que resulta beneficioso para el mantenimiento a largo plazo.


**Conclusión**

Gracias por la oportunidad de completar esta evaluación técnica. Disfruté los desafíos presentados y los encontré como un ejercicio valioso para aplicar mis habilidades y conocimientos. Creo que las soluciones que proporcioné se alinean con los requisitos especificados y demuestran mi capacidad para resolver problemas de manera efectiva utilizando mi experiencia técnica.

Si hay alguna parte de mi envío que requiere aclaración, o si se necesita más información, no dude en ponerse en contacto conmigo. Espero tener la posibilidad de discutir mi solicitud con más detalle.

Atentamente,

Sebastian Gomez Quintero.
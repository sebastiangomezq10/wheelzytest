# Soluci�n de prueba t�cnica

Hola soy Sebasti�n Gomez Quintero y a continuaci�n ver�s la soluci�n a la prueba t�cnica

# Responde la pregunta 1
La respuesta a este punto se encuentra a continuaci�n en sql. La parte EF se puede ver durante la ejecuci�n del proyecto iniciando la API o ejecutando la prueba unitaria.

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


## Consulta para mostrar la informaci�n

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

�Qu� har�as si tuvieras datos que no cambian con frecuencia pero que se usan pr�cticamente todo el tiempo?

Para datos que no cambian con frecuencia pero a se accede con frecuencia, implementar una estrategia de almacenamiento en cach� suele ser el mejor enfoque. El almacenamiento en cach� implica almacenar datos en un almac�n de datos de acceso r�pido despu�s de la primera recuperaci�n para que las solicitudes futuras de datos puedan atenderse r�pidamente, sin la necesidad de realizar consultas repetidas a la base de datos.

Hay varias estrategias de almacenamiento en cach� a considerar:

1. **Cach� en Memoria**: Almacenar datos directamente en la memoria de la aplicaci�n puede acelerar significativamente el acceso. Se pueden utilizar tecnolog�as como Redis, Memcached o incluso cach� en proceso con una herramienta como MemoryCache en .NET.
2. **Cach� Distribuido**: Para aplicaciones que se escalan horizontalmente a trav�s de m�ltiples servidores, un cach� distribuido asegura que los datos en cach� est�n disponibles para todas las instancias de la aplicaci�n. Este enfoque es beneficioso para aplicaciones nativas de la nube que requieren alta disponibilidad y escalabilidad.
3. **Cach� Persistente**: A veces, los datos pueden necesitar ser almacenados en cach� por per�odos m�s largos y deber�an persistir m�s all� de los reinicios de la aplicaci�n. En tales casos, usar un almac�n persistente como un cach� basado en archivos o una base de datos puede ser adecuado.
4. **Invalidaci�n de Cach�**: Es crucial tener una estrategia para invalidar el cach� cuando los datos finalmente cambian. Esto se puede hacer mediante pol�ticas de expiraci�n, etiquetas de versi�n o actualizaciones de cach� impulsadas por eventos.
5. **Cach� CDN**: Para aplicaciones web, datos est�ticos como im�genes, CSS y archivos JavaScript se pueden almacenar en cach� en una Red de Distribuci�n de Contenidos (CDN) para mejorar los tiempos de carga para usuarios en diferentes ubicaciones geogr�ficas.

El almacenamiento en cach� puede reducir la carga de la base de datos, disminuir la latencia y mejorar la capacidad de respuesta general de una aplicaci�n.

# Responde la pregunta 3

## Refactorizaci�n del m�todo UpdateCustomersBalanceByInvoices

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

Este m�todo tiene varios problemas que podr�an optimizarse:

1. **Rendimiento**: El m�todo llama a `dbContext.SaveChanges()` dentro de un bucle, lo que da como resultado una transacci�n de base de datos separada para cada factura. Esto es altamente ineficiente.
    
2. **Manejo de errores**: No hay manejo de errores para garantizar que se encuentre al "cliente" antes de intentar actualizar el saldo.
    
3. **M�ltiples llamadas a la base de datos**: el m�todo consiste en recuperar al cliente de la base de datos uno por uno en un bucle, lo que puede provocar problemas de rendimiento debido a la cantidad de llamadas a la base de datos.
    
4. **Sin manejo de concurrencia**: si la aplicaci�n se utiliza en un entorno de subprocesos m�ltiples, la falta de manejo de concurrencia podr�a generar datos inconsistentes.

	
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

En el m�todo mejorado:

- El contexto de la base de datos `SaveChanges` se llama una vez despu�s de todas las actualizaciones, lo que reduce la cantidad de transacciones y mejora el rendimiento.
- Los clientes se recuperan en una �nica consulta, lo que reduce los viajes de ida y vuelta a la base de datos.
- Se agrega la verificaci�n de existencia de clientes para evitar excepciones de referencia nula.
- Prepara el m�todo para una posible optimizaci�n de la actualizaci�n por lotes y un mejor manejo de errores.

# Responde la pregunta 4

## Implementaci�n del m�todo GetOrders

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

Bill, del Departamento de Control de Calidad, le asign� una tarea de alta prioridad que indica que hay un error cuando alguien cambia el estado de "Aceptado" a "Recogido". Define c�mo proceder�as, paso a paso, hasta crear el Pull Request.

### Enfoque paso a paso para resolver el error
1. **Reproduzca el problema**:
    
- Comience intentando reproducir el error en un entorno de desarrollo controlado. Utilice los pasos exactos proporcionados por el equipo de control de calidad o los pasos que provocaron el error informado.
- Verificar que el error ocurra consistentemente con los pasos descritos y en condiciones similares.
2. **Identifique el error**:
    
     - Examinar el c�digo responsable de cambiar el estado de "Aceptado" a "Recogido". Es probable que esto involucre controladores de eventos o servicios que gestionan transiciones de estado.
     - Verifique si existen condiciones, validaciones o errores en la l�gica que puedan provocar el problema informado.
3. **Depuraci�n**:
    
     - Utilice herramientas de depuraci�n para recorrer el c�digo y monitorear el estado de la aplicaci�n a medida que pasa de "Aceptada" a "Recogida".
     - Preste especial atenci�n a cualquier modificaci�n que se realice en la base de datos o en el estado de la aplicaci�n durante esta transici�n.
4. **Escribe una prueba**:
    
     - Escribe una prueba unitaria que falle debido al error. Esto no s�lo confirma la presencia del error sino que tambi�n garantiza que una vez solucionado, el error no vuelva a ocurrir.
     - La prueba debe imitar el cambio de estado y comprobar el resultado esperado.
5. **Implemente la soluci�n**:
    
     - Una vez identificada la fuente del error, modifique el c�digo para rectificar el problema. Esto podr�a implicar corregir la l�gica de la transici�n de estado, manejar las excepciones de manera m�s adecuada o ajustar la l�gica empresarial relacionada.
     - Aseg�rese de que los cambios no interrumpan ninguna funcionalidad existente mediante la ejecuci�n de pruebas existentes.
6. **Ejecutar pruebas**:
    
     - Ejecute la prueba reci�n creada junto con otras pruebas relevantes para garantizar que la soluci�n funcione como se esperaba y no introduzca nuevos problemas.
     - Aseg�rese de que todas las pruebas pasen antes de continuar con los siguientes pasos.
7. **Revisi�n de c�digo**:
    
     - Crear una solicitud de extracci�n (PR) para los cambios. Proporcione una descripci�n clara de los cambios y el motivo de los mismos en el PR.
     - Solicite una revisi�n del c�digo a sus pares. Aseg�rese de que al menos otro desarrollador revise los cambios para garantizar la calidad y la mantenibilidad.
8. **Implementar los cambios**:
    
     - Una vez aprobado, fusionar el PR en la rama principal de desarrollo.
     - Siga los procedimientos est�ndar de su equipo para implementar cambios en el entorno de producci�n, lo que podr�a implicar etapas adicionales de prueba.
9. **Verifique la soluci�n en producci�n**:
    
     - Una vez implementada, verifique que la soluci�n resuelva el problema en el entorno de producci�n sin causar efectos secundarios inesperados.
10. **Documente la resoluci�n**:

      - Actualizar cualquier documentaci�n que refleje los cambios realizados. Esto podr�a incluir documentaci�n t�cnica, manuales de usuario o notas de la versi�n.
      - La documentaci�n ayuda a los futuros desarrolladores a comprender los cambios y las razones detr�s de ellos, lo que resulta beneficioso para el mantenimiento a largo plazo.


**Conclusi�n**

Gracias por la oportunidad de completar esta evaluaci�n t�cnica. Disfrut� los desaf�os presentados y los encontr� como un ejercicio valioso para aplicar mis habilidades y conocimientos. Creo que las soluciones que proporcion� se alinean con los requisitos especificados y demuestran mi capacidad para resolver problemas de manera efectiva utilizando mi experiencia t�cnica.

Si hay alguna parte de mi env�o que requiere aclaraci�n, o si se necesita m�s informaci�n, no dude en ponerse en contacto conmigo. Espero tener la posibilidad de discutir mi solicitud con m�s detalle.

Atentamente,

Sebastian Gomez Quintero.
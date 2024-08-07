Bu solutionda **Saga Choreography Pattern** ve **Saga Orchestration Pattern** yaklaşımları kullanılarak oluşturulan sipariş distrubuted transaction akışı RabbitMQ Message Broker ve MassTransaction kütüphanesi ile microservisler arasında asenkron yönetilmiştir.

Distributed transaction  birden fazla veri kaynağı veya sistem üzerinde gerçekleştirilen bir işlemi ifade eder, yani bu senaryoda ayrı microservislerdeki tüm işlemlerin bir bütün olarak başarılı veya başarısız olarak ele alınması gerekir.

<br>

**Servisler**

- **Order**: Sipariş kaydı oluşturularak entitysini veritabanında oluşturarak, akışı Stock servisine event göndererek ilerleten microservisdir.
  
**OrderCreatedEvent =>** Sipariş oluşturulduktan sonra gönderilen event

- **Stock**: Stok durumunu kontrol ederek, stoklar işleme uygunsa Payment servisine event gönderir, uygun değilse sipariş kaydı durumunun buna göre güncellenmesi için Order sevisine event gönderiren microservisdir.
  
**StockUpdatedEvent =>** Stok başarılı şekilde güncellenirse gönderilen event 

**StockNotUpdatedEvent =>** Stok başarılı şekilde güncellenemeze veya stok yetersizse gönderilen event 

- **Payment**: Ödemeyi kontrol ederek, ödeme başarılıysa Order servisine Order kaydını başarılı statüsüne çekecek ödeme başarılı eventını Order servisine gönderir. Ödeme başarısızsa stoklarda compensable transaction yapılması için Stock servisine event gönderen ve sipariş kaydının durumunun buna göre güncellenmesi için Order sevisine event gönderen microservisdir.
  
**PaymentSuccessfulEvent =>** Ödeme başarılı olursa gönderilen event

**PaymentFailedEvent =>** Ödeme başarısız olursa gönderilen event   

<br><br>

**Saga Choreography Pattern**

Saga Choreography pattern, mikroservice mimarisi içinde distrubuted transactionları yönetmek için kullanılan bir desen türüdür. Bu yaklaşımda, işlemler birbirini takip eden microservisler arasında dağıtılır ve her bir hizmet kendi işleyişinden sorumludur. Her mikro hizmet, işlem tamamlandığında bir sonraki hizmeti tetikleyen bir olay yayınlar. Yani, işlemler arasında bir merkezi kontrol veya yönlendirme noktası yoktur. Her microservis, yaptığı işlemin başarılı/başarısız olduğuna karar vererek tekrar kuyruğa bir dönüş yapar.

![image](https://github.com/user-attachments/assets/443b8426-78e4-4315-b9a7-73e9c887f8fb)


**Order** servinde aşağıdaki evenları publish eder veya subscribe olur.

OrderCreatedEvent(publish)      

PaymentFailedEvent(subscribe: order-payment-failed-queue)

PaymentSuccessfulEvent(subscribe: order-payment-successful-queue")

StockNotUpdatedEvent(subscribe: order-stock-not-updated-queue)

<br>

**Stock** servinde aşağıdaki evenları publish eder veya subscribe olur.

OrderCreatedEvent(subscribe: stock-order-created-queue) 

PaymentFailedEvent(subscribe: stock-payment-failed-queue)

StockUpdatedEvent(publish)		  

StockNotUpdatedEvent(publish)     

<br>

**Payment** servinde aşağıdaki evenları publish eder veya subscribe olur.

StockUpdatedEvent(subscribe: payment-stock-updated-queue)			

PaymentSuccessfulEvent(publish)     

PaymentFailedEvent(publish)        



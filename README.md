Bu solutionda **Saga Choreography Pattern** ve **Saga Orchestration Pattern** yaklaşımları kullanılarak oluşturulan sipariş distrubuted transaction akışı RabbitMQ Message Broker ve MassTransit kütüphanesi ile microservisler arasında asenkron yönetilmiştir.

Distributed transaction  birden fazla veri kaynağı veya sistem üzerinde gerçekleştirilen bir işlemi ifade eder, yani bu senaryoda ayrı microservislerdeki tüm işlemlerin bir bütün olarak başarılı veya başarısız olarak ele alınması gerekir.

<br>

**Servisler**

- **Order**: Sipariş kaydı oluşturularak entitysini veritabanında oluşturarak, akışı Stock servisine event göndererek ilerleten microservisdir.
  
**OrderCreatedEvent =>** Sipariş oluşturulduktan sonra tetiklenen olaydır

- **Stock**: Stok durumunu kontrol ederek, stoklar işleme uygunsa Payment servisine event gönderir, uygun değilse sipariş kaydı durumunun buna göre güncellenmesi için Order sevisine event gönderiren microservisdir.
  
**StockUpdatedEvent =>** Stok başarılı şekilde güncellenirse tetiklenen olaydır 

**StockNotUpdatedEvent =>** Stok başarılı şekilde güncellenemeze veya stok yetersizse tetiklenen olaydır 

- **Payment**: Ödemeyi kontrol ederek, ödeme başarılıysa Order servisine Order kaydını başarılı statüsüne çekecek ödeme başarılı eventını Order servisine gönderir. Ödeme başarısızsa stoklarda compensable transaction yapılması için Stock servisine event gönderen ve sipariş kaydının durumunun buna göre güncellenmesi için Order sevisine event gönderen microservisdir.
  
**PaymentSuccessfulEvent =>** Ödeme başarılı olursa tetiklenen olaydır

**PaymentFailedEvent =>** Ödeme başarısız olursa tetiklenen olaydır   

<br><br><br>

# Saga Choreography Pattern

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

<br>

Bir servisin dinleyeceği bütün eventlar ayrı ayrı Consumer class olarak oluşturup, Program.cs'de dinleyeceği kuyruk ile birlike tanımlanır.

![image](https://github.com/user-attachments/assets/985891ee-10be-4830-a375-80b0890ff8a1)


Consumer class MassTransit IConsumer interface'i üzerinden implemente edilerek subscribe olunan event tetiklendiğinde meydana gelecek akış içerisindeki Consume metodu içerisinde tanımlanır. 

![image](https://github.com/user-attachments/assets/d6309d8c-4ced-4680-bb7b-7420de6d9f20)

<br>

Belirli bir microservisteki belirli bir hedefe mesaj göndermek için MassTransit Send metodu, bir mesajı birden fazla mikro hizmete dağıtmak MassTransit Publish metodu için kullanılır.

![image](https://github.com/user-attachments/assets/dfd853ff-ea02-4c43-9048-dd1cb0430505)
![image](https://github.com/user-attachments/assets/f9ad8258-886d-49e0-bdf2-be95cec63644)


<br><br><br>

# Saga Orchestration Pattern

Saga Orchestration pattern, mikroservice mimarisi içinde distrubuted transactionları yönetmek için kullanılan diğer bir desen türüdür. Bu yaklaşımda, merkezi bir kontrol mekanizması kullanılarak microservisler arasında işlemler yönetilir. Merkezi koordinatör, işlemlerin adım adım nasıl ilerleyeceğini belirler ve mikroservislere gerekli eventları gönderir. Yapılan işlemin başarılı/başarısız olduğu koordinatör doğrulaması sonucunda karar verilir.

<br>

Sipariş sürecindeki akış yöntemek için bir WorkerService oluşturulur ve WorkerService içerisinde MassTransit kütüphanesinin MassTransitStateMachine sınıfından türetilerek OrderStateMachine sınıfı oluşturulur. İş akışındaki farklı aşamalar State'ler ile temsil edilir, her durum, bir iş sürecinin belirli bir noktasını tanımlar. Durumlardaki değişikliklikler Event'lar ile tetiklenir. Bir durumdan diğerine geçiş de bu sınıf içerisinde tanımlanır.

![image](https://github.com/user-attachments/assets/9d2c8596-aba6-45a8-9382-82b828e9609b)

Her bir akış OrderStateInstance entitysi olarak sistemde InMemory veya veritabanı kaydı olarak tutulur. Biz bu örnekte akışı veri tabanından gözlemlemek için veri tabanı üzerinde OrderStateInstance kayıtlarını tutulmuştur.

![image](https://github.com/user-attachments/assets/2c4120ac-1cdd-40e6-8c6e-0d13f497fa8e)


MassTransit'teki During, When, ve TransitionTo yapıları ile, State Machine içerisinde çeşitli durumlar arasında geçişler tanımlanır. During ile hangi tanımlı durumda, When ile Hangi event tetiklendiğinin koşulu belirtilir, TransitionTo yeni durumun hangisi olduğu belirtilir. Koşul ve state geçişi belirlendikten sonra Publish veya Send ile akış ilerletilir.

![image](https://github.com/user-attachments/assets/afe1b94a-5b63-4daf-9916-e51795d37143)


<br>

Mevcut senaryoda **OrderStateMachine**'in içerdiği sipariş sürecindeki olayları, durumları ve bu durumlar arasındaki geçişler aşağıda görülebilir;

![image](https://github.com/user-attachments/assets/fd59e01d-cfe7-49d9-bd2e-d2b1074802ed)


**Durumlar (States)**

**OrderCreated**: Siparişin başarıyla oluşturulduğu ve işlemeye hazır olduğu durumdur.

**StockUpdated:** Stok güncellemelerinin başarıyla tamamlandığı durumdur.

**StockNotUpdated**: Stok güncellemelerinin başarısız olduğu ve bu nedenle siparişin işlenemediği durumdur.

**PaymentSuccessful**: Ödeme işleminin başarıyla tamamlandığı durumdur.

**PaymentFailed**: Ödeme işleminin başarısız olduğu durumdur.

<br>

**Olaylar (Events)**

**OrderCreatedStateEvent =>** Yeni bir sipariş oluşturulduğunda tetiklenen olaydır

**StockUpdatedStateEvent =>** Stok güncellemeleri başarıyla tamamlandığında tetiklenen olaydır

**StockNotUpdatedStateEvent =>** Stok güncellemeleri başarısız olduğunda tetiklenen olaydır

**PaymentSuccessfulStateEvent =>** Ödeme işlemi başarıyla tamamlandığında tetiklenen olaydır

**PaymentFailedStateEvent =>** Ödeme işlemi başarısız olduğunda tetiklenen olaydır

**StockUpdatedPaymentRequestEvent =>** Stock servisinde stok güncelleme akışını tetikleyen olaydır

**StockNotUpdatedOrderRequestEvent =>** Order servisinde stok durumundan dolayı siparişi başarısız duruma çekme akışını tetikleyen olaydır

**PaymentSuccessfulOrderRequestEvent =>** Order servisinde başarılı ödemeden dolayı sipariş durumunu başarılıya çekme akışını tetikleyen olaydır

**PaymentFailedToAllRequestEvent =>** Ödeme başarısız olduğunda bütün servislerde compensable transaction akşını tetikleyen olaydır

<br>

**Geçişler (Transitions)**

**OrderCreated ==(StockUpdatedStateEvent)==> StockUpdated**: Stok başarıyla güncellendiğinde gerçekleşir. Payment servisinin dinlediği "payment-stock-updated-queue" kuyruğuna StockUpdatedPaymentRequestEvent olayını gönderir.

**OrderCreated ==(StockNotUpdatedStateEvent)==> StockNotUpdated**: Stok güncellenemediğinde gerçekleşir. Order servisinin dinlediği "order-stock-not-updated-queue" kuyruğuna StockNotUpdatedOrderRequestEvent olayını gönderir.

**StockUpdated ==(PaymentSuccessfulStateEvent)==> PaymentSuccessful**: Ödeme başarılı olduğunda gerçekleşir. Order servisinin dinlediği "order-payment-successful-queue" kuyruğuna PaymentSuccessfulOrderRequestEvent olayını gönderir.

**StockUpdated ==(PaymentFailedStateEvent)==> PaymentFailed**: Ödeme başarısız olduğunda gerçekleşir. Bütün servislere PaymentFailedToAllRequestEvent olayı publish edilir.

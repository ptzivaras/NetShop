## Security

### ⚠️ NEVER COMMIT SECRETS TO VERSION CONTROL!
Προφανης ο λογος που τα εχουμε local και οχι ανεβασμενα..
- Database passwords
- API keys (Stripe, SendGrid, etc.)
- JWT signing keys
- Connection strings with credentials
- OAuth client secrets
- Encryption keys

### ⚠️ IDOR — τι είναι;
Insecure Direct Object Reference. Απλό παράδειγμα: πας στο /orders/123 και βλέπεις την παραγγελία σου. Αλλαγάς στο URL 123 → 124 και βλέπεις την παραγγελία άλλου χρήστη. Αυτό είναι IDOR. Εσύ το "φτιάχνεις" ελέγχοντας currentUserId == requestedUserId στους controllers.


### ⚠️ Repository Pattern
**Why is direct DB access in controllers a problem?**
-Security Risks:
 1.Controllers mix business logic with data access - harder to audit security
 2.SQL injection easier if queries built in controllers
 3.Can't centrally validate/sanitize data access patterns
 
-Testing Problems:
 1.Can't unit test controllers without real database
 2.Harder to mock data for different scenarios

-Architecture Issues:
 1.Controllers become fat/complex
 2.Duplicate data access code across controllers
 3.Harder to maintain and change DB logic
 
-Repository Pattern Benefits:
 1.Single place to review all database queries (security audit)
 2.Easy to add logging/monitoring of data access
 3.Can enforce security policies (row-level security, data masking)
 4.Makes testing easier with mock repositories
 
### ⚠️ Exceptions επιστρέφουν sensitive data.
-Global Error Handling ισως να μην τα προσεχει σωστα η γενικα τα exceptions

### ⚠️ Http και οχι Https
- What is the problem how it helps

### IDOR Vulnerability 
- What is the problem how it helps

### Hardcoded URLs 
-Why is this a problem?

### Rate Limiting 
- What is the problem how it helps

### CORS 
- What is the problem how it helps

### Authorization - Added [Authorize(Roles = "Admin")]
- What is the problem how it helps?
- Why mine is not working properly?

### Identity + Cookies vs Identity + JWT VS Oath2
- ?

### CQRS
- ?

### Indexes
- What are they what i need know from them where to put them what is the tradeoff

### Stored Procedure
- How it helps us what problem solves where to put mine.

### Code-First approach with migrations

### Transaction Types in .Net/Entity Framework Core
-   **1. TransactionScope (System.Transactions)**
	Τι είναι:

	Distributed transaction manager που δουλεύει σε ADO.NET επίπεδο
	Μπορεί να τυλίξει πολλαπλά DbContext instances ή ακόμα και διαφορετικές βάσεις δεδομένων
	Χρησιμοποιεί Distributed Transaction Coordinator (DTC) αν χρειαστεί
	Πλεονεκτήματα:

	✅ Δουλεύει με οποιαδήποτε ADO.NET connection (όχι μόνο EF Core)
	✅ Μπορεί να συντονίσει πολλαπλά DbContext objects στο ίδιο transaction
	✅ Declarative syntax - τυλίγεις τον κώδικα σε using block και αυτόματα κάνει rollback αν πετάξει exception
	✅ Ιδανικό για cross-cutting operations (π.χ. order + stock update + cart clearing που χρησιμοποιούν διαφορετικά repositories)
	Μειονεκτήματα:

	❌ Λίγο πιο "βαρύ" σε performance για απλές περιπτώσεις
	❌ Χρειάζεται AsyncFlowOption.Enabled για async code
	❌ Μπορεί να κάνει escalate σε distributed transaction (αν δεν το ρυθμίσεις σωστά)
	Πότε να το χρησιμοποιήσεις:

	✅ Όταν θέλεις να τυλίξεις πολλαπλά repository operations σε ένα transaction
	✅ Όταν θέλεις service-layer transactions (όχι μόνο στο DbContext)
	✅ Όταν μπορεί να χρειαστεί να επεκταθεί σε διαφορετικές βάσεις δεδομένων μελλοντικά
	
- **2. DbContext.Database.BeginTransaction() (EF Core Built-in)**
	Τι είναι:

	Native EF Core transaction API
	Δουλεύει μόνο με ένα συγκεκριμένο DbContext instance
	Πιο lightweight από το TransactionScope
	Πλεονεκτήματα:

	✅ Ταχύτερο και πιο lightweight
	✅ Καλύτερο performance για simple scenarios
	✅ Έχεις explicit control πάνω στο transaction (commit, rollback, savepoints)
	✅ Full async/await support χωρίς extra configuration
	Μειονεκτήματα:

	❌ Δουλεύει μόνο με ένα DbContext - δεν μπορεί να συντονίσει πολλαπλά contexts
	❌ Πρέπει να το περάσεις ως parameter σε repositories/services αν θέλεις να share το ίδιο transaction
	❌ Περισσότερος boilerplate code (explicit commit/rollback)
	Πότε να το χρησιμοποιήσεις:

	✅ Όταν όλες οι operations γίνονται στο ίδιο DbContext
	✅ Όταν θέλεις maximum performance για απλά scenarios
	✅ Όταν θέλεις fine-grained control (savepoints, manual commit timing)
	
- **3. Implicit Transactions (SaveChanges behavior)**
	Τι είναι:

	Default EF Core behavior: Το SaveChanges() τυλίγει όλα τα pending changes σε ένα atomic transaction
	Δεν χρειάζεται να γράψεις κώδικα - γίνεται αυτόματα
	Πλεονεκτήματα:

	✅ Μηδενικός κώδικας - just call SaveChanges()
	✅ Automatic rollback αν fail
	✅ Καλύτερο για single-operation CRUD
	Μειονεκτήματα:

	❌ Δουλεύει μόνο μέσα σε ΕΝΑ SaveChanges() call
	❌ Δεν μπορεί να τυλίξει πολλαπλά SaveChanges() calls
	❌ Δεν μπορεί να τυλίξει non-EF operations (file writes, external API calls, etc.)
	Πότε να το χρησιμοποιήσεις:

	✅ Απλά CRUD operations (Create/Update/Delete ενός entity)
	✅ Όταν ΟΛΑ τα changes γίνονται tracked στο DbContext και κάνεις SaveChanges μια φορά
	✅ Default επιλογή για > 90% των περιπτώσεων
  
- **Γιατί επέλεξα TransactionScope στο OrderService:**
	Το Scenario:
	// OrderService.CreateOrderAsync()
	1. Create Order entity (+ OrderItems)
	2. Update Product stock (LOOP: πολλαπλά products)
	   - Για κάθε product: UpdateAsync() → SaveChanges()
	3. Clear shopping cart
	4. Final SaveChanges()
	
-  **Γιατί ΟΧΙ Implicit Transaction (SaveChanges only):**

	❌ Κάνω πολλαπλά SaveChanges() μέσα στο loop (ένα για κάθε product stock update)
	❌ Αν fail στο 3ο product: Τα πρώτα 2 products έχουν ήδη saved → Data corruption
	❌ Order θα υπάρχει, αλλά κάποια products δεν έχουν μειωθεί → Overselling
	Γιατί ΟΧΙ BeginTransaction():

	❌ Θα έπρεπε να pass το transaction object σε κάθε repository method (ProductRepository.UpdateAsync, CartRepository.DeleteAsync)
	❌ Repositories θα γίνουν tightly coupled με transaction management
	❌ Δύσκολο refactoring αν προσθέσω νέα operations μελλοντικά
	❌ Θα έπρεπε να inject το DbContext στο OrderService αντί το Repository (παραβιάζει Repository Pattern)
	Γιατί ΝΑΙ TransactionScope:

	✅ Δουλεύει σε Service Layer level - τα repositories δεν ξέρουν τίποτα για transactions
	✅ Τυλίγει όλα τα SaveChanges() calls αυτόματα (order creation + 5 product updates + cart clear)
	✅ Declarative - απλό using block, automatic rollback on exception
	✅ Future-proof - αν μελλοντικά προσθέσω logging σε external DB ή file writes, το transaction θα τα καλύψει
	✅ Clean Separation of Concerns - Repositories κάνουν data access, Service κάνει transaction orchestration
### Concurrency
- **Optimistic Concurrency Control** - `[Timestamp] RowVersion` on Product entity
  - Prevents lost updates when multiple users modify same product simultaneously
  - DbUpdateConcurrencyException handling with user-friendly error messages


### Cache
- What are my options.

### Why 3 layers
- What are my options.

### Cookies
- What i need to know about them.

### Γιατί RestSharp;
✅ Απλούστερος κώδικας (δεν χρειάζεται manual JSON deserialization)
✅ Built-in error handling
✅ Automatic query parameter encoding

### AJAX
- Τι ειναι αυτο? εχω τετοιο?
# 🎓 ZamETF – Studentski informacioni sistem

## 📌 O projektu
ZamETF je web aplikacija razvijena kao verzija tzv. Zamgera – studentskog informacionog sistema za podršku nastavnom procesu na fakultetu.  
Sistem centralizuje podatke o studentima, profesorima i predmetima, omogućava digitalnu administraciju i olakšava komunikaciju između svih aktera.

## 🚀 Funkcionalnosti
- ✅ **Prijava ispita putem sistema** – studenti se prijavljuju na ispitne rokove direktno kroz aplikaciju.  
- 📂 **Predaja i pregled zadaća** – upload zadaća, pregled statusa i bodovanja.  
- 📝 **Kreiranje i ocjenjivanje zadaća** – profesori kreiraju zadatke, ocjenjuju i ostavljaju komentare.  
- 🎯 **Evidencija prisustva i unos rezultata ispita** – profesori unose bodove i prisustvo studenata.  
- 🔔 **Slanje notifikacija** – automatske obavijesti o ocjenama, zadacima i prijavama ispita.  
- 👩‍💻 **Administracija korisnika i predmeta** – dodavanje, izmjena i brisanje podataka o studentima, profesorima i predmetima.  
- 📑 **Generisanje dokumenata i uvjerenja (PDF)** – potvrde o statusu, položenim ispitima, upisanom semestru.  
- 📊 **Statistika** – izračun prosjeka, prolaznosti i ocjena po predmetima.  

## 👥 Akteri sistema
- **Student** – prijava ispita, predaja zadaća, pregled ocjena i notifikacija.  
- **Profesor** – kreiranje i ocjenjivanje zadaća, unos rezultata ispita, pregled statistike.  
- **Studentska služba** – generisanje i slanje potvrda, pregled statistike.  
- **Administrator** – upravljanje korisnicima, predmetima i sistemskim notifikacijama.  

## 🛠️ Tehnologije
- ASP.NET MVC (C#)  
- Entity Framework (ORM)  
- SQL Server (baza podataka)  
- HTML, CSS, JavaScript (frontend)  
- iTextSharp / QuestPDF (generisanje PDF dokumenata)  

## ⚙️ Konfiguracija baze
Konekcijski string:

```
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=ZamETF;User Id=USERNAME;Password=PASSWORD;Trusted_Connection=False;MultipleActiveResultSets=true"
  }
}

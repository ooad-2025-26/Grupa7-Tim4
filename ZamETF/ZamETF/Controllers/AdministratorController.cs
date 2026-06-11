// ============================================================
// ZAMIJENITI u AdministratorController.cs
// ============================================================

// POST: Administrator/KreirajKorisnika
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> KreirajKorisnika(string ime, string prezime,
    string email, string indeks, int godinaStudija, string privilegije, string lozinka)
{
    // Ako admin nije unio email, automatski ga generiši
    var finalEmail = string.IsNullOrWhiteSpace(email)
        ? await GenerirajEmail(ime, prezime)
        : email.Trim();

    // Provjeri da li email već postoji (bitno kad admin ručno unosi)
    if (!string.IsNullOrWhiteSpace(email))
    {
        var postojeciEmail = await _userManager.FindByEmailAsync(finalEmail);
        if (postojeciEmail != null)
        {
            TempData["Greska"] = $"Korisnik s emailom '{finalEmail}' već postoji.";
            return RedirectToAction(nameof(UnosIzmjena));
        }
    }

    var username = await GenerirajUsername(ime, prezime);
    IdentityResult result;

    if (privilegije == "Student")
    {
        var student = new Student
        {
            Ime = ime,
            Prezime = prezime,
            UserName = username,
            Email = finalEmail,
            Indeks = indeks,
            GodinaStudija = godinaStudija,
            Uloga = Uloga.Student,
            EmailConfirmed = true
        };
        result = await _userManager.CreateAsync(student, lozinka);
        if (result.Succeeded)
            await _userManager.AddToRoleAsync(student, "Student");
    }
    else if (privilegije == "Profesor")
    {
        var profesor = new Profesor
        {
            Ime = ime,
            Prezime = prezime,
            UserName = username,
            Email = finalEmail,
            Uloga = Uloga.Profesor,
            EmailConfirmed = true
        };
        result = await _userManager.CreateAsync(profesor, lozinka);
        if (result.Succeeded)
            await _userManager.AddToRoleAsync(profesor, "Profesor");
    }
    else if (privilegije == "Administrator")
    {
        var admin = new Administrator
        {
            Ime = ime,
            Prezime = prezime,
            UserName = username,
            Email = finalEmail,
            Uloga = Uloga.Administrator,
            EmailConfirmed = true
        };
        result = await _userManager.CreateAsync(admin, lozinka);
        if (result.Succeeded)
            await _userManager.AddToRoleAsync(admin, "Administrator");
    }
    else
    {
        var sluzba = new StudentskaSluzba
        {
            Ime = ime,
            Prezime = prezime,
            UserName = username,
            Email = finalEmail,
            Uloga = Uloga.StudentskaSluzba,
            EmailConfirmed = true
        };
        result = await _userManager.CreateAsync(sluzba, lozinka);
        if (result.Succeeded)
            await _userManager.AddToRoleAsync(sluzba, "StudentskaSluzba");
    }

    if (result.Succeeded)
    {
        var emailNapomena = string.IsNullOrWhiteSpace(email)
            ? $"{finalEmail} (automatski generisan)"
            : finalEmail;
        TempData["Uspjeh"] = $"Korisnik kreiran! Email: {emailNapomena} | Username: {username} | Lozinka: {lozinka}";
    }
    else
    {
        TempData["Greska"] = string.Join(", ", result.Errors.Select(e => e.Description));
    }

    return RedirectToAction(nameof(UnosIzmjena));
}


// POST: Administrator/IzmijeniKorisnika
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> IzmijeniKorisnika(
    int id,
    string ime, string prezime,
    string username, string email,
    string indeks, string odsjek,
    string jmbg, DateTime? datumRodjenja,
    string imeOca, string imeMajke,
    string mjesto, string ciklus,
    string tipStudija, string status,
    int godinaStudija, int semestar)
{
    var student = await _context.Studenti.FindAsync(id);
    if (student == null)
    {
        TempData["Greska"] = "Student nije pronađen.";
        return RedirectToAction(nameof(UnosIzmjena));
    }

    // Osnovna Identity polja — username i email zahtijevaju poseban tretman
    student.Ime = ime;
    student.Prezime = prezime;

    // Username — promijeni samo ako se razlikuje
    if (!string.IsNullOrWhiteSpace(username) && username != student.UserName)
    {
        var setUsernameResult = await _userManager.SetUserNameAsync(student, username);
        if (!setUsernameResult.Succeeded)
        {
            TempData["Greska"] = "Greška pri promjeni korisničkog imena: " +
                string.Join(", ", setUsernameResult.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(UnosIzmjena));
        }
    }

    // Email — promijeni samo ako se razlikuje
    if (!string.IsNullOrWhiteSpace(email) && email != student.Email)
    {
        var setEmailResult = await _userManager.SetEmailAsync(student, email);
        if (!setEmailResult.Succeeded)
        {
            TempData["Greska"] = "Greška pri promjeni emaila: " +
                string.Join(", ", setEmailResult.Errors.Select(e => e.Description));
            return RedirectToAction(nameof(UnosIzmjena));
        }
        // Nakon SetEmailAsync, email je postavljen ali nije potvrđen — potvrdi odmah
        student.EmailConfirmed = true;
    }

    // Student-specifična polja
    student.Indeks = indeks;
    student.Odsjek = odsjek;
    student.JMBG = jmbg;
    student.ImeOca = imeOca;
    student.ImeMajke = imeMajke;
    student.MjestoPrebivalisca = mjesto;
    student.Ciklus = ciklus;
    student.TipStudija = tipStudija;
    student.StatusStudenta = status;
    student.GodinaStudija = godinaStudija;
    student.Semestar = semestar;

    if (datumRodjenja.HasValue)
        student.DatumRodjenja = datumRodjenja.Value;

    var result = await _userManager.UpdateAsync(student);

    if (result.Succeeded)
        TempData["Uspjeh"] = $"Podaci za {student.Ime} {student.Prezime} su uspješno izmijenjeni!";
    else
        TempData["Greska"] = "Greška pri snimanju: " +
            string.Join(", ", result.Errors.Select(e => e.Description));

    return RedirectToAction(nameof(UnosIzmjena));
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using carp_tm_2025.Data;
using carp_tm_delegati;
using carp_tm_2025.Models;
using Microsoft.AspNetCore.Authorization;
using iText.Kernel.Pdf;
using X.PagedList.Extensions;
using iText.IO.Font.Constants;
using iText.Kernel.Font;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Layout;
using Microsoft.Extensions.Hosting;
using iText.Kernel.Events;
using iText.Kernel.Geom;
using iText.Kernel.Pdf.Canvas;
using iText.Layout.Layout;
using iText.Layout.Renderer;
using Excel.FinancialFunctions;
using static Org.BouncyCastle.Utilities.Test.FixedSecureRandom;
using iText.Kernel.Utils.Annotationsflattening;


namespace carp_tm_2025.Controllers
{
    [Authorize(Roles = "CARP_TM\\CARP_TM_READ")]
    public class PensionarsController : Controller
    {
        private readonly carp_tm_2025Context _context;

        private readonly IWebHostEnvironment _envp;


        public static string titlu_raport_pens;
        public PensionarsController(carp_tm_2025Context context, IWebHostEnvironment environment)
        {
            _context = context;
            _envp = environment;

        }

        // GET: Pensionars
        public async Task<IActionResult> Index(string searchterm = "99999")
        {           

            // de aici
            int nrcarnet_OK;
            decimal soldimp_show = 0;
            string nrcarnetDP = "";
            int nrcarnetDPint = 0;

            ViewBag.nrcarnet = searchterm;

            if (searchterm.Contains("."))
                searchterm = searchterm.Replace(".", string.Empty);


            if (int.TryParse(searchterm, out nrcarnet_OK) == false)
            {
                ViewData["Error"] = "Introduceti o valoare numerica pentru nr carnet";
                return View("Error");
            }

            var pensionari_index = await _context.Pensionars.Where(pi => pi.nrcarnet == nrcarnet_OK).FirstOrDefaultAsync();
           
            if (nrcarnet_OK != 99999)
            {
                if (pensionari_index == null)
                {
                    ViewData["Error"] = "Acest nr de carnet nu exista";
                    return View("Error");
                }

                //if delegat redirect RC delegati tb uncoment
                string userIdPensIndex = User.Identity.Name.Trim();
                var get_iduser_pensIndex_db = _context.utilizatoris
                 .Where(u => u.nume_user == userIdPensIndex).SingleOrDefault();
                int get_iduser_PensIndex = get_iduser_pensIndex_db.IDUser;
                //int get_iduser_PensIndex = 0;

                if (get_iduser_PensIndex > 10)
                    if (pensionari_index.id_delegat != get_iduser_PensIndex)
                    {
                        ViewData["Error"] = "Acest nr de carnet nu exista in zona dumneavoastra";
                        return View("Error");
                    }

                ViewData["soldimp_desf"] = 0;

                if (pensionari_index.id_stare == 3) ViewData["Stare"] = "DEBITOR-DECEDAT";
                if (pensionari_index.id_stare == 4) ViewData["Stare"] = "DEBITOR-POPRIRI";
                if (pensionari_index.id_stare == 0) ViewData["Stare"] = "-";
                if (pensionari_index.id_stare == 1) ViewData["Stare"] = "DECEDAT";
                if (pensionari_index.id_stare == 2) ViewData["Stare"] = "EXCLUS";


                if (pensionari_index.id_stare == 30) ViewData["Stare"] = "DEBITOR-DECEDAT";


                if (pensionari_index.id_stare == 51) @ViewBag.Stare = "EXCLUDERE";
                if (pensionari_index.id_stare == 52) @ViewBag.Stare = "RETRAGERE";




                if (pensionari_index.soldimp > 0)
                {
                    DateTime get_dataimp = (DateTime)pensionari_index.dataimp;

                    var get_soldimp_desf =await  _context.desfasurator_rate
                        .Where(gsd => ((gsd.nrcarnet == nrcarnet_OK) && (gsd.data_rata.Month == DateTime.Now.Month) && (gsd.data_rata.Year == DateTime.Now.Year))).FirstOrDefaultAsync();                    

                    var ch_get_soldimp = await _context.Chitantas.Where(cgs => (cgs.nrcarnet == nrcarnet_OK) && (cgs.data.Month == DateTime.Now.Month) && (cgs.data.Year == DateTime.Now.Year) && (cgs.rata > 0)).ToListAsync();

                    if (get_soldimp_desf != null)
                    {
                        if (ch_get_soldimp.Count() >= 1) soldimp_show = get_soldimp_desf.sold_imp;
                        if (ch_get_soldimp.Count() == 0) soldimp_show = get_soldimp_desf.sold_imp + get_soldimp_desf.rata_de_plata;
                    }

                    if ((get_dataimp.Month == DateTime.Now.Month) && (get_dataimp.Year == DateTime.Now.Year)) soldimp_show = pensionari_index.soldimp;

                    var ch_get_credit =await _context.Chitantas.Where(cgs => (cgs.nrcarnet == nrcarnet_OK) && (cgs.data.Month == DateTime.Now.Month) && (cgs.data.Year == DateTime.Now.Year) && (cgs.credit_imprumut > 0)).ToListAsync();
                    if (ch_get_credit.Count() >= 1) soldimp_show = pensionari_index.soldimp;

                    if (soldimp_show - (pensionari_index.soldimp - pensionari_index.debit_imprumut) == 0.01M)
                        soldimp_show = soldimp_show - 0.01M;

                    if (soldimp_show - (pensionari_index.soldimp - pensionari_index.debit_imprumut) == -0.01M)
                        soldimp_show = soldimp_show + 0.01M;
                    //pa code si teorie 
                    ViewData["soldimp_desf"] = soldimp_show.ToString("N2");

                }

                if (pensionari_index.soldimp < 0) ViewData["soldimp_desf"] = pensionari_index.soldimp.ToString("N2");


                if (pensionari_index.id_stare==4)                        
                {
                    nrcarnetDP = pensionari_index.nrcarnet.ToString().Remove(0, 4);
                    nrcarnetDPint = Int32.Parse(nrcarnetDP);
                    var pensDP = _context.Pensionars.Where(ppd => ppd.nrcarnet == nrcarnetDPint).SingleOrDefault();
                    ViewData["solddob"] = pensDP.solddobanda.ToString("N2"); 
                }




            }

            ViewData["sunt_date"] = 1;
          //  if (pensionari_index == null) ViewData["sunt_date"] = 0;

            return View(pensionari_index);

            //  return View(await _context.Pensionars.ToListAsync());
        }

        // GET: Pensionars/Details/5
      

        // GET: Pensionars/Create
        public IActionResult Create()
        {
            return View();
        }


        // POST: Pensionars/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("nrcarnet,nume,prenume,cnp,judet,localitate,strada,nr,bloc,ap,telefon,email,nrcupon,pensie,taxainscriere,datainscriere,lunaimp,soldimp,credit_imprumut,debit_imprumut,restimp,acimp,dataimp,LLL,desfasurator,sold_461,soldcotiz,incascotiz,lunaajdeces,EXCL,soldimp2016,soldcotiz2016,soldajdeces2016,lunaajdeces2016,sold_DP2016,credit_imprumut2016,debit_imprumut2016,sold_dobanda2016,soldajdeces,solddobanda,sold_DP,luna_DP,soldtaxainscr,id_delegat,id_stare")] Pensionar pensionar)
        {
            

            if (ModelState.IsValid)
            {

                var cek_nrcarnet= await _context.Pensionars
                   .Where(cnc => cnc.nrcarnet == pensionar.nrcarnet).SingleOrDefaultAsync();

                if (cek_nrcarnet !=null)
                {
                    ViewData["Error"] = "Mai exista acest nr de carnet";
                    return View("Error");

                }


                //insert id delegat
                string userIdinsPens = User.Identity.Name.Trim();
                var get_iduser_insPens_db = _context.utilizatoris
                   .Where(u => u.nume_user == userIdinsPens).SingleOrDefault();
                int get_iduser_insPens = get_iduser_insPens_db.IDUser;
                pensionar.id_delegat = get_iduser_insPens;



                pensionar.datainscriere=DateTime.Now;
                pensionar.lunaajdeces = DateTime.Now.AddMonths(-1);
                pensionar.lunaimp= Convert.ToDateTime("1/1/2001");
                pensionar.prenume = "-";

                _context.Pensionars.Add(pensionar);
                await _context.SaveChangesAsync();

                update_conturi_analitice(pensionar.nrcarnet, pensionar.nume);


                _context.erori_103_109.Add(new erori_103_109
                {
                    nrcarnet = pensionar.nrcarnet,
                    SI_103 = 0,
                    SI_109 = 0,
                    credit_103 = 0,
                    credit_109 = 0,
                    debit_103 = 0,
                    debit_109 = 0,
                    dobanda_neplatita = 0,
                    SF_103 = 0,
                    SF_109 = 0
                });
                await _context.SaveChangesAsync();

                return RedirectToAction("Index", new { searchterm = pensionar.nrcarnet });
               
            }

            return View(pensionar);


        }


        [Authorize(Roles = "CARP_TM\\CARP_TM_WRITE")]
        // GET: Pensionars/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var pensionar = await _context.Pensionars.FindAsync(id);
            if (pensionar == null)
            {
                return NotFound();
            }
            return View(pensionar);
        }


        [Authorize(Roles = "CARP_TM\\CARP_TM_WRITE")]
        // POST: Pensionars/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("nrcarnet,nume,prenume,cnp,judet,localitate,strada,nr,bloc,ap,telefon,email,nrcupon,pensie,taxainscriere,datainscriere,lunaimp,soldimp,credit_imprumut,debit_imprumut,restimp,acimp,dataimp,LLL,desfasurator,sold_461,soldcotiz,incascotiz,lunaajdeces,EXCL,soldimp2016,soldcotiz2016,soldajdeces2016,lunaajdeces2016,sold_DP2016,credit_imprumut2016,debit_imprumut2016,sold_dobanda2016,soldajdeces,solddobanda,sold_DP,luna_DP,soldtaxainscr,id_delegat,id_stare,sold_461_2016,sold_462_2016")] Pensionar pensionar)
        {          
            //NOV 2024 NET8                                       
          


            string userId = User.Identity.Name;
            var getiduserp = await _context.utilizatoris
                .Where(u => u.nume_user == userId).SingleOrDefaultAsync();


            if (id != pensionar.nrcarnet)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(pensionar);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!PensionarExists(pensionar.nrcarnet))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }

                insert_audit_pensionari(getiduserp.IDUser, pensionar.nrcarnet, pensionar.soldimp, pensionar.soldcotiz, pensionar.lunaajdeces, pensionar.lunaimp);


                return RedirectToAction("Index", new { searchterm = pensionar.nrcarnet });
            }



            return View(pensionar);
        }

      


        public ActionResult cautare_partida(string searchterm = "xxx")
        {
            string userIdPensFind = User.Identity.Name.Trim();
            var get_iduser_pensFind_db = _context.utilizatoris
             .Where(u => u.nume_user == userIdPensFind).SingleOrDefault();
            int get_iduser_PensFind = get_iduser_pensFind_db.IDUser;
           
            var pensionarnrcarnet = _context.Pensionars
                           .Where(p => p.nume.StartsWith(searchterm) && p.nume.Contains(searchterm));

            if(get_iduser_PensFind >10)
                pensionarnrcarnet = _context.Pensionars
                           .Where(p => p.nume.StartsWith(searchterm) && p.nume.Contains(searchterm) && (p.id_delegat==get_iduser_PensFind));

            ViewBag.numemic = searchterm;
            return View(pensionarnrcarnet);
        }


        public  int update_conturi_analitice(int nrcarnetCA, string numeCA)
        {
           
            _context.conturi_analitice.Add(new conturi_analitice
            {
                // id_cont_analitic = Int32.Parse("113" + pidca_record.nrcarnet.ToString()),
                id_cont_analitic = Int32.Parse("113" + nrcarnetCA.ToString()),
                id_cont_sintetic = 101,
                nr_cont_analitic = "113." + nrcarnetCA.ToString(),
                explicatie_nr_cont_analitic = numeCA,
            });
            _context.SaveChanges();


            _context.conturi_analitice.Add(new conturi_analitice
            {
                // id_cont_analitic = Int32.Parse("113" + pidca_record.nrcarnet.ToString()),
                id_cont_analitic = Int32.Parse("114" + nrcarnetCA.ToString()),
                id_cont_sintetic = 102,
                nr_cont_analitic = "114." + nrcarnetCA.ToString(),
                explicatie_nr_cont_analitic = numeCA

            });
            _context.SaveChanges();


            _context.conturi_analitice.Add(new conturi_analitice
            {
                // id_cont_analitic = Int32.Parse("113" + pidca_record.nrcarnet.ToString()),
                id_cont_analitic = Int32.Parse("2678" + nrcarnetCA.ToString()),
                id_cont_sintetic = 103,
                nr_cont_analitic = "2678." + nrcarnetCA.ToString(),
                explicatie_nr_cont_analitic = numeCA

            });
            _context.SaveChanges();


            _context.conturi_analitice.Add(new conturi_analitice
            {
                // id_cont_analitic = Int32.Parse("113" + pidca_record.nrcarnet.ToString()),
                id_cont_analitic = Int32.Parse("6791" + nrcarnetCA.ToString()),
                id_cont_sintetic = 109,
                nr_cont_analitic = "26791." + nrcarnetCA.ToString(),
                explicatie_nr_cont_analitic = numeCA

            });
            _context.SaveChanges();


            _context.conturi_analitice.Add(new conturi_analitice
            {
                // id_cont_analitic = Int32.Parse("113" + pidca_record.nrcarnet.ToString()),
                id_cont_analitic = Int32.Parse("1012" + nrcarnetCA.ToString()),
                id_cont_sintetic = 105,
                nr_cont_analitic = "1012." + nrcarnetCA.ToString(),
                explicatie_nr_cont_analitic = numeCA

            });
            _context.SaveChanges();



            return 0;
        }


        public async Task<ActionResult> CD(int ID_CD_in = 0, int luna_in = 0,  int? page = 1)
        {
            //NOV 2024 NET8
            DateTime now = DateTime.Now;
            string pdfdd = "";
            DateTime dataimp_dd = Convert.ToDateTime("1/1/2001");
            string nrcarnetDD = "";

            int ID_CD = ID_CD_in;
            int luna = luna_in;

            ViewData["luna"] = luna;
            ViewData["ID_CD"] = ID_CD;
         

            decimal CD_credit_A = 0;
            decimal CD_debit_A = 0;
            decimal CD_debit = 0;
            decimal CD_credit = 0;
            decimal SI_CD = 0, SF_CD = 0;
            string pdfCD = "";
            decimal total_SI_CD = 0, total_SF_CD = 0;
            decimal total_credit_CD = 0, total_debit_CD = 0;
            string titlu_CD = "";
            int i = 0;
            decimal si_credit = 0;
            decimal si_debit = 0;


            var CA_CD =await _context.conturi_analitice.Where(cd => cd.id_cont_sintetic == ID_CD).OrderBy(cd => cd.id_cont_analitic).ToListAsync();


            var CD_list = new List<CD>();
            var CD_list_ordonat = CD_list.OrderBy(cdl => cdl.nr_cont_CD);
            var get_CA =await _context.conturi_analitice.Where(gca => gca.id_cont_analitic == 0).SingleOrDefaultAsync();
            var cek_CD_list = CD_list.Where(cdl => cdl.nr_cont_CD == "000").SingleOrDefault();


            string webRootPath = _envp.WebRootPath.ToString() + "\\PDF\\";
            PdfWriter writer = new PdfWriter(webRootPath + "CD.pdf");
            PdfDocument pdfDoc = new PdfDocument(writer);
            Document document = new Document(pdfDoc, iText.Kernel.Geom.PageSize.A4.Rotate(), false);

            document.SetMargins(100f, 20f, 50f, 20f);

            Style normal = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normal.SetFont(font).SetFontSize(14);

            //header
            Paragraph header1 = new Paragraph("C.A.R. Pensionari Turnu Magurele")
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(13).AddStyle(normal);

            Paragraph header2 = new Paragraph("cod fiscal 7230589")
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFontSize(13)
                .AddStyle(normal);

            var table = new Table(6, true);
            Style normalTbl = new Style();
            PdfFont fontTbl = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normalTbl.SetFont(fontTbl).SetFontSize(13);

            Cell nrc_titlu = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("nr cont")).SetBold().AddStyle(normalTbl);

            Cell expl_nrc_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                     .Add(new Paragraph("explicatie nr cont")).SetBold().AddStyle(normalTbl);

            Cell si_titlu = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("sold initial")).AddStyle(normalTbl).SetBold();


            Cell debit_titlu = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("debit")).AddStyle(normalTbl).SetBold();


            Cell credit_titlu = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("credit")).AddStyle(normalTbl).SetBold();

            Cell sf_titlu = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("sold final")).AddStyle(normalTbl).SetBold();

            table.AddHeaderCell(nrc_titlu);
            table.AddHeaderCell(expl_nrc_titlu);
            table.AddHeaderCell(si_titlu);
            table.AddHeaderCell(debit_titlu);
            table.AddHeaderCell(credit_titlu);
            table.AddHeaderCell(sf_titlu);
          


            if (luna != 0)
            {
                //pune debit luna    
                var CD_debit_db =await _context.registru_jurnal.Where(cdd => (cdd.id_CS_debitor == ID_CD) && (cdd.data.Month == luna))
            .GroupBy(cdda => new
            {
                cdda.id_CA_debitor,
            })
                 .Select(g => new
                 {
                     id_CA_D = g.Key.id_CA_debitor,
                     total_credit = 0,
                     total_debit = g.Sum(h => h.debit),
                     sortare = "H"
                 }
                 ).ToListAsync();

                foreach (var CDD in CD_debit_db)
                {
                    i = i + 1;

                    get_CA =await _context.conturi_analitice.Where(gca => (gca.id_cont_analitic == CDD.id_CA_D)).SingleOrDefaultAsync();

                    CD_list.Add(new CD
                    {
                        id_CD = i,
                        nr_cont_CD = get_CA.nr_cont_analitic,
                        explicatie_nr_cont_CD = get_CA.explicatie_nr_cont_analitic,
                        SI_CD = 0,
                        credit_CD = 0,
                        debit_CD = CDD.total_debit,
                        SF_CD = 0,
                    });

                    _context.SaveChanges();
                }

                //pune credit luna

                var CD_credit_db =await _context.registru_jurnal.Where(cdd => (cdd.id_CS_creditor == ID_CD) && (cdd.data.Month == luna))
                .GroupBy(cdda => new
                {
                    cdda.id_CA_creditor,
                })
                     .Select(g => new
                     {
                         id_CA_C = g.Key.id_CA_creditor,
                         total_credit = g.Sum(h => h.credit),
                         total_debit = 0,
                         sortare = "H"
                     }
                     ).ToListAsync();

                foreach (var CDC in CD_credit_db)
                {
                    i = i + 1;

                    get_CA = _context.conturi_analitice.Where(gca => (gca.id_cont_analitic == CDC.id_CA_C)).SingleOrDefault();

                    cek_CD_list = CD_list.Where(cdl => cdl.nr_cont_CD.Trim() == get_CA.nr_cont_analitic.Trim()).SingleOrDefault();

                    if (cek_CD_list == null)
                        CD_list.Add(new CD
                        {
                            id_CD = i,
                            nr_cont_CD = get_CA.nr_cont_analitic,
                            explicatie_nr_cont_CD = get_CA.explicatie_nr_cont_analitic,
                            SI_CD = 0,
                            credit_CD = CDC.total_credit,
                            debit_CD = 0,
                            SF_CD = 0,

                        });

                    if (cek_CD_list != null)
                        cek_CD_list.credit_CD = CDC.total_credit;

                    _context.SaveChanges();
                }

                //calcul SI

                //pune debit luna    
                var SI_CD_db =await _context.registru_jurnal.Where(cdd => (cdd.id_CS_debitor == ID_CD) && (cdd.data.Month < luna))
                .GroupBy(cdda => new
                {
                    cdda.id_CA_debitor,
                })
                     .Select(g => new
                     {
                         id_CA_D = g.Key.id_CA_debitor,
                         total_credit = 0,
                         total_debit = g.Sum(h => h.debit),
                         sortare = "H"
                     }
                     ).ToListAsync();

                //!!!begin an trebuie SI
                if (SI_CD_db.Count() > 0)
                    foreach (var CDSI in SI_CD_db)
                    {
                        i = i + 1;
                        get_CA =await _context.conturi_analitice.Where(gca => (gca.id_cont_analitic == CDSI.id_CA_D)).SingleOrDefaultAsync();

                        cek_CD_list = CD_list.Where(cdl => cdl.nr_cont_CD.Trim() == get_CA.nr_cont_analitic.Trim()).SingleOrDefault();
                        si_debit = 0;
                        if (ID_CD != 18) si_debit = CDSI.total_debit;
                        if (ID_CD == 18) si_debit = -CDSI.total_debit;

                        if (cek_CD_list == null)
                            CD_list.Add(new CD
                            {
                                id_CD = i,
                                nr_cont_CD = get_CA.nr_cont_analitic,
                                explicatie_nr_cont_CD = get_CA.explicatie_nr_cont_analitic,
                                SI_CD = si_debit,
                                credit_CD = 0,
                                debit_CD = 0,
                                SF_CD = 0,
                            });


                        if (cek_CD_list != null)
                            cek_CD_list.SI_CD = CDSI.total_debit;

                        _context.SaveChanges();
                    }


                //SI get credit            
                var SI_CDC_db = await _context.registru_jurnal.Where(cdd => (cdd.id_CS_creditor == ID_CD) && (cdd.data.Month < luna))
                .GroupBy(cdda => new
                {
                    cdda.id_CA_creditor,
                })
                     .Select(g => new
                     {
                         id_CA_D = g.Key.id_CA_creditor,
                         total_credit = g.Sum(h => h.credit),
                         total_debit = 0,
                         sortare = "H"
                     }
                     ).ToListAsync();

                //!!!begin an trebuie SI
                if (SI_CDC_db.Count() > 0)
                    foreach (var CDCSI in SI_CDC_db)
                    {
                        i = i + 1;
                        get_CA = _context.conturi_analitice.Where(gca => (gca.id_cont_analitic == CDCSI.id_CA_D)).SingleOrDefault();

                        cek_CD_list = CD_list.Where(cdl => cdl.nr_cont_CD.Trim() == get_CA.nr_cont_analitic.Trim()).SingleOrDefault();


                        si_credit = 0;
                        if (ID_CD == 18) si_credit = CDCSI.total_credit;
                        if (ID_CD != 18) si_credit = -CDCSI.total_credit;

                        if (cek_CD_list == null)
                            CD_list.Add(new CD
                            {
                                id_CD = i,
                                nr_cont_CD = get_CA.nr_cont_analitic,
                                explicatie_nr_cont_CD = get_CA.explicatie_nr_cont_analitic,
                                SI_CD = si_credit,
                                credit_CD = 0,
                                debit_CD = 0,
                                SF_CD = 0,
                            });


                        if (cek_CD_list != null)
                        {
                            if (ID_CD == 18)
                                cek_CD_list.SI_CD = cek_CD_list.SI_CD + CDCSI.total_credit;
                            if (ID_CD != 18)
                                cek_CD_list.SI_CD = cek_CD_list.SI_CD - CDCSI.total_credit;
                        }

                        _context.SaveChanges();
                    }

                decimal si_2016 = 0;

                //foreach (pend
                int idCDstare = 0;
                if (ID_CD == 26) idCDstare = 3;
                if (ID_CD == 27) idCDstare = 4;
                //var pensCD =await _context.Pensionars.Where(pcd => (pcd.id_stare == idCDstare) && (pcd.sold_461>0) && (pcd.nrcarnet<461410000) && (pcd.nrcarnet!=46141063)).OrderBy(pcd=>pcd.nrcarnet).ToListAsync();
                var pensCD = await _context.Pensionars.Where(pcd => (pcd.id_stare == idCDstare) && (pcd.sold_461_2016 > 0) ).OrderBy(pcd => pcd.nrcarnet).ToListAsync();
                
                if (ID_CD==18) pensCD = await _context.Pensionars.Where(pcd => pcd.sold_462_2016 > 0).OrderBy(pcd => pcd.nrcarnet).ToListAsync();


                //                if (ID_CD!=18)
                int cekE2025 = 0;
                foreach (Pensionar pcd in pensCD)
                {
                   

                    si_2016 = 0;
                     if (ID_CD==18) get_CA= await _context.conturi_analitice.Where(gca => gca.nr_cont_analitic == "4621." + pcd.nrcarnet.ToString().Trim()).SingleOrDefaultAsync();


                    if ((ID_CD == 26) ||  (ID_CD == 27)) get_CA =await _context.conturi_analitice.Where(gca => gca.id_cont_analitic == pcd.nrcarnet).SingleOrDefaultAsync();
                 cek_CD_list =  CD_list_ordonat.Where(ccdl => ccdl.nr_cont_CD == get_CA.nr_cont_analitic).SingleOrDefault();

                 si_2016 = pcd.sold_461_2016;
                 if (ID_CD==18) si_2016 = pcd.sold_462_2016;



                    cekE2025 = 0;
                    if (cek_CD_list == null)
                    {
                        CD_list.Add(new CD
                            {
                                id_CD = i,
                                nr_cont_CD = get_CA.nr_cont_analitic,
                                explicatie_nr_cont_CD = get_CA.explicatie_nr_cont_analitic,
                                SI_CD =si_2016,
                                credit_CD = 0,
                                debit_CD = 0,
                                SF_CD = 0,
                            });
                        cekE2025 = 1;
                    }

                    // (cek_CD_list != null)
                    if (cekE2025==0)
                    {                    
                       
                        if (ID_CD != 18) cek_CD_list.SI_CD = cek_CD_list.SI_CD + si_2016;
                        if (ID_CD==18) cek_CD_list.SI_CD = cek_CD_list.SI_CD + si_2016;                        
                    }

                    _context.SaveChanges();                     
                
                }

                //pax
                //CD_list = db.CD.OrderBy(c => c.nr_cont_CD);
                foreach (CD cdl in CD_list_ordonat)
                {
                    SF_CD = 0;
                    if (ID_CD == 18) SF_CD = cdl.SI_CD + cdl.credit_CD - cdl.debit_CD;
                    if (ID_CD != 18) SF_CD = cdl.SI_CD - cdl.credit_CD + cdl.debit_CD;

                    cdl.SF_CD = SF_CD;
                    _context.SaveChanges();

                    if ((cdl.SI_CD != 0) || ((cdl.SI_CD == 0) && (cdl.debit_CD != 0)))
                    {

                     Cell nrc = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph(cdl.nr_cont_CD).AddStyle(normalTbl));
                     table.AddCell(nrc);

                    Cell expl_nrc = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .Add(new Paragraph(cdl.explicatie_nr_cont_CD).AddStyle(normalTbl));
                    table.AddCell(expl_nrc);

                    Cell sicd = new Cell(1, 1)
                            .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                             .Add(new Paragraph(cdl.SI_CD.ToString("N2")).AddStyle(normalTbl));
                    table.AddCell(sicd);

                    Cell debitcd = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .Add(new Paragraph(cdl.debit_CD.ToString("N2")).AddStyle(normalTbl));
                    table.AddCell(debitcd);

                     Cell creditcd = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .Add(new Paragraph(cdl.credit_CD.ToString("N2")).AddStyle(normalTbl));
                    table.AddCell(creditcd);

                    Cell sfcd = new Cell(1, 1)
                           .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                            .Add(new Paragraph(cdl.SF_CD.ToString("N2")).AddStyle(normalTbl));
                    table.AddCell(sfcd);

                    }


                }

                total_SI_CD = CD_list.Sum(cdl => cdl.SI_CD);
                ViewData["total_SI_CD"] = total_SI_CD.ToString("N2");

                total_debit_CD = CD_list.Sum(cdl => cdl.debit_CD);
                ViewData["total_debit_CD"] = total_debit_CD.ToString("N2");

                total_credit_CD = CD_list.Sum(cdl => cdl.credit_CD);
                ViewData["total_credit_CD"] = total_credit_CD.ToString("N2");

                total_SF_CD = CD_list.Sum(cdl => cdl.SF_CD);
                ViewData["total_SF_CD"] = total_SF_CD.ToString("N2");

            }                     

        
            if (ID_CD == 18) titlu_CD = " CREDITORI ";
            if (ID_CD == 26) titlu_CD = " DEBITORI DECEDATI ";
            if (ID_CD == 27) titlu_CD = " DEBITORI POPRIRI ";

            titlu_raport_pens = "Situatie" + titlu_CD + "pentru luna " + luna;


            ViewData["titluCD0"] = titlu_CD;
            ViewData["titluCD"]= titlu_raport_pens;
            ViewData["titlutCD"] = "Total " + titlu_raport_pens;        


            Paragraph ptsi = new Paragraph();
            ptsi.Add(new Text("Sold intial ").SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal).SetBold());
            ptsi.Add(new Text(total_SI_CD.ToString("N2")).SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal));
            Paragraph ptcd = new Paragraph();

            ptcd.Add(new Text("Total debit ").SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(14).AddStyle(normal).SetBold());
            ptcd.Add(new Text(total_debit_CD.ToString("N") + ", ").SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(14).AddStyle(normal));

            ptcd.Add(new Text("Total credit ").SetTextAlignment(TextAlignment.LEFT)
             .SetFontSize(14).AddStyle(normal).SetBold());
            ptcd.Add(new Text(total_credit_CD.ToString("N2") + ", ").SetTextAlignment(TextAlignment.LEFT)
             .SetFontSize(14).AddStyle(normal));

            Paragraph ptsf = new Paragraph();
            ptsf.Add(new Text("Sold final ").SetTextAlignment(TextAlignment.LEFT)
           .SetFontSize(14).AddStyle(normal).SetBold());
            ptsf.Add(new Text(total_SF_CD.ToString("N2")).SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(14).AddStyle(normal));


            document.Add(ptsi);
            document.Add(ptcd);
            document.Add(ptsf);

            document.Add(new Paragraph(" "));

            document.Add(table);
            table.Complete();

            document.Add(new Paragraph(" "));

            i = 1;
            for (i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                float x = 20f;
                float y = pageSize.GetTop() - 20;
                document.ShowTextAligned(header1, x, y, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
                document.ShowTextAligned(header2, x, y - 20f, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
            }


            int numberOfPages = pdfDoc.GetNumberOfPages();

            for (i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                float y = 20f;
                float x = pageSize.GetBottom() + 20;
                document.ShowTextAligned(new Paragraph(DateTime.Now.ToString("dd.MM.yyyy") + "                                                                                                                                                                                      " + "pagina " + i + " din " + numberOfPages).AddStyle(normal), x, y, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);

            }
                       
            TableHeaderEventHandler handler = new TableHeaderEventHandler(document);
            pdfDoc.AddEventHandler(PdfDocumentEvent.END_PAGE, handler);

            document.Close();


            var pageNumber = page ?? 1;
            ViewData["CD"] = CD_list_ordonat.ToList().ToPagedList(pageNumber, 15);

            return View(ViewData["CD"]);

        }


        private class TableHeaderEventHandler : IEventHandler
        {
            private Table table;
            private float tableHeight;
            private Document doc;



            public TableHeaderEventHandler(Document doc)
            {
                this.doc = doc;
                InitTable();

                TableRenderer renderer = (TableRenderer)table.CreateRendererSubTree();
                renderer.SetParent(new DocumentRenderer(doc));

                // Simulate the positioning of the renderer to find out how much space the header table will occupy.
                LayoutResult result = renderer.Layout(new LayoutContext(new LayoutArea(0, PageSize.A4)));
                tableHeight = result.GetOccupiedArea().GetBBox().GetHeight();
            }

            public void HandleEvent(Event currentEvent)
            {
                PdfDocumentEvent docEvent = (PdfDocumentEvent)currentEvent;
                PdfDocument pdfDoc = docEvent.GetDocument();
                PdfPage page = docEvent.GetPage();
                PdfCanvas canvas = new PdfCanvas(page.NewContentStreamBefore(), page.GetResources(), pdfDoc);
                PageSize pageSize = pdfDoc.GetDefaultPageSize();
                float coordX = pageSize.GetX() + doc.GetLeftMargin();
                float coordY = pageSize.GetTop() - doc.GetTopMargin();
                float width = pageSize.GetWidth() - doc.GetRightMargin() - doc.GetLeftMargin();
                float height = GetTableHeight();
                Rectangle rect = new Rectangle(coordX, coordY, width, height);

                new Canvas(canvas, rect)
                    .Add(table)
                    .Close();
            }

            public float GetTableHeight()
            {
                return tableHeight;
            }

            private void InitTable()
            {
                Style normalh = new Style();
                PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
                normalh.SetFont(font).SetFontSize(14);


                Cell h3 = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER)
                         .Add(new Paragraph(titlu_raport_pens).SetFontSize(15).SetBold().AddStyle(normalh));
                h3.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);


                Cell h4 = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("").SetFontSize(13).SetBold().AddStyle(normalh));
                h4.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);

                Cell h5 = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                    .Add(new Paragraph("").SetFontSize(13).SetBold().AddStyle(normalh));
                h5.SetBorder(border: iText.Layout.Borders.Border.NO_BORDER);

                table = new Table(1).UseAllAvailableWidth();
                table.AddCell(h3);
                table.AddCell(h4);
                table.AddCell(h5);

            }
        }

        public int insert_audit_pensionari(int User, int nrcarnet, decimal soldimp, decimal soldcotiz, DateTime lunaajdeces, DateTime lunaimp)
        {

            //IULIE 2023 TABLETS MVC
            _context.Audit_Pensionar.Add(new Audit_Pensionar
            {

                tip_operatie = "Edit",
                data = DateTime.Now,
                utilizator = User,
                nrcarnet = nrcarnet,
                sold_imprumut = soldimp,
                sold_cotizatie = soldcotiz,
                luna_ajutor_deces = lunaajdeces,

                ultima_plata_rata = lunaimp

            });

            _context.SaveChanges();
            return 0;


        }



        public async Task<ActionResult>  debitori_4612(int luna_d4612_in = 0, int? page = 1)
        {
            // NOV 2024 NET 8
            int luna_d4612 = 0;
            int an_d4612 = DateTime.Now.Year;
            string nrcarnet_d4612_str;
            int nrcarnet_d4612_int;
            int id4612 = 0;
            string pdfd4612 = "";
            string pdfd4612n = "";
            decimal trei_rate = 0;
            DateTime dataimp_d4612;
            decimal debit_4612 = 0;
            decimal total_d4612;
            decimal sold_d4612=0;
            string lunaimp_d4612;

            luna_d4612 = luna_d4612_in;
            if (DateTime.Now.Month == luna_d4612_in) luna_d4612 = luna_d4612_in - 1;
            if (luna_d4612 == 0) luna_d4612 = 12;
            if (luna_d4612 == 0) an_d4612 = an_d4612 - 1;

            ViewData["luna_d4612_in"] = luna_d4612_in;
            
            var pens_d4612 =await _context.Pensionars.Where(pd4612 => pd4612.nrcarnet == 3893).SingleOrDefaultAsync();

            
            
            
            var rj_D4612 =await _context.registru_jurnal.Where(rjd4612 => (rjd4612.data.Month == luna_d4612) && (rjd4612.id_CS_debitor == 107) && (rjd4612.data.Year == an_d4612)).OrderBy(rjd4612 => rjd4612.id_CA_debitor).ToListAsync();

            var D4612_list = new List<D_4612>();
            var D4612_list_ordonat = D4612_list.OrderBy(d4612 => d4612.nrcarnet_d4612);

            var pens_end_desf =await _context.Pensionars.Where(ped => (ped.desfasurator > 0) && (ped.desfasurator <= luna_d4612)).ToListAsync();


            string webRootPath = _envp.WebRootPath.ToString() + "\\PDF\\";
            PdfWriter writer = new PdfWriter(webRootPath + "debitori_4612.pdf");
            PdfDocument pdfDoc = new PdfDocument(writer);
            Document document = new Document(pdfDoc, iText.Kernel.Geom.PageSize.A4.Rotate(), false);

            document.SetMargins(100f, 20f, 50f, 20f);

            Style normal = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normal.SetFont(font).SetFontSize(14);

            //header
            Paragraph header1 = new Paragraph("C.A.R. Pensionari Turnu Magurele")
                .SetTextAlignment(TextAlignment.LEFT)
                .SetFontSize(13).AddStyle(normal);

            Paragraph header2 = new Paragraph("cod fiscal 7230589")
               .SetTextAlignment(TextAlignment.LEFT)
               .SetFontSize(13)
                .AddStyle(normal);

            int nrcol = 6;
            if (luna_d4612_in == DateTime.Now.Month ) nrcol = 8;
            if (luna_d4612_in == DateTime.Now.Month-1) nrcol = 8;

            var table = new Table(nrcol, true);
            Style normalTbl = new Style();
            PdfFont fontTbl = PdfFontFactory.CreateFont(StandardFonts.TIMES_ROMAN);
            normalTbl.SetFont(fontTbl).SetFontSize(11);

            Cell nrc_titlu = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("nr carnet")).SetBold().AddStyle(normalTbl);

            Cell nume_titlu = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER)
                     .Add(new Paragraph("nume si prenume")).SetBold().AddStyle(normalTbl);            


            Cell localitate_titlu = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("localitate")).AddStyle(normalTbl).SetBold();


            Cell dataimp_titlu = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("data imprumut")).AddStyle(normalTbl).SetBold();

            Cell acimp_titlu = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("valoare imprumut")).AddStyle(normalTbl).SetBold();

            Cell soldimp_titlu = new Cell(1, 1)
            .SetTextAlignment(TextAlignment.CENTER)
             .Add(new Paragraph("sold imprumut")).AddStyle(normalTbl).SetBold();

            Cell lunaimp_titlu = new Cell(1, 1)
               .SetTextAlignment(TextAlignment.CENTER)
                .Add(new Paragraph("ultima plata")).AddStyle(normalTbl).SetBold();

            Cell debit_titlu = new Cell(1, 1)
              .SetTextAlignment(TextAlignment.CENTER)
               .Add(new Paragraph("debit imprumut")).AddStyle(normalTbl).SetBold();

            table.AddHeaderCell(nrc_titlu);
            table.AddHeaderCell(nume_titlu);
            table.AddHeaderCell(localitate_titlu);
            table.AddHeaderCell(dataimp_titlu);
            table.AddHeaderCell(acimp_titlu);

            if (nrcol == 8) table.AddHeaderCell(soldimp_titlu);
            if (nrcol == 8) table.AddHeaderCell(lunaimp_titlu);

            table.AddHeaderCell(debit_titlu);








            foreach (registru_jurnal rjd4612 in rj_D4612)
            {
                nrcarnet_d4612_str = rjd4612.id_CA_debitor.ToString();
                nrcarnet_d4612_str = nrcarnet_d4612_str.Remove(0, 4);
                nrcarnet_d4612_int = Int32.Parse(nrcarnet_d4612_str.Trim());               
                pens_d4612 =await _context.Pensionars.Where(pd4612 => pd4612.nrcarnet == nrcarnet_d4612_int).SingleOrDefaultAsync();

                trei_rate = 0;
                if (pens_d4612.acimp > 10000) trei_rate = pens_d4612.acimp / 60;
                if (pens_d4612.acimp <= 10000) trei_rate = pens_d4612.acimp / 36;
                if (pens_d4612.acimp <= 3000) trei_rate = pens_d4612.acimp / 24;


                id4612 = 1;
                id4612 = id4612 + 1;
                dataimp_d4612 = (DateTime)pens_d4612.dataimp;

                debit_4612 = 0;
                debit_4612 = rjd4612.SF_CA_debit;
                // if ((luna_d4612 == DateTime.Now.Month - 1) || (luna_d4612 == 12))
                if (luna_d4612_in == DateTime.Now.Month)
                    debit_4612 = pens_d4612.debit_imprumut;

                sold_d4612 = 0;
                sold_d4612 = pens_d4612.soldimp - pens_d4612.debit_imprumut;
                if (luna_d4612_in == DateTime.Now.Month-1)
                    sold_d4612=rjd4612.SF_CA_credit-rjd4612.SF_CA_debit;
                lunaimp_d4612 = "";
                lunaimp_d4612=pens_d4612.lunaimp.ToString("dd.MM.yyyy");
                if ((pens_d4612.lunaimp.Month == DateTime.Now.Month) && (pens_d4612.lunaimp.Year == DateTime.Now.Year) && (luna_d4612_in!=DateTime.Now.Month))  lunaimp_d4612 = "" ;

                if ((3 * trei_rate < debit_4612) && (debit_4612>0)                   )
                    if ((pens_d4612.soldimp != 0) || (pens_d4612.debit_imprumut != 0))
                        D4612_list.Add(new D_4612
                        {
                            id_d4612 = id4612,
                            nrcarnet_d4612 = nrcarnet_d4612_int,
                            nume_d4612 = pens_d4612.nume,
                            localitate_d4612 = pens_d4612.localitate,
                            data_impr_d4612 = dataimp_d4612,
                            val_impr_d4612 = pens_d4612.acimp,
                            rest_impr_d4612 = pens_d4612.restimp,
                            //sold_impr_d4612 = pens_d4612.soldimp - pens_d4612.debit_imprumut,
                            sold_impr_d4612 = sold_d4612,
                            lunaimp_impr_d4612 = lunaimp_d4612,
                            debit_impr_d4612 = debit_4612
                        });
            }

            if (luna_d4612_in != 0)
                foreach (Pensionar ped in pens_end_desf)
                {

                    id4612 = 1;
                    id4612 = id4612 + 1;
                    dataimp_d4612 = (DateTime)ped.dataimp;

                    D4612_list.Add(new D_4612
                    {
                        id_d4612 = id4612,
                        nrcarnet_d4612 = ped.nrcarnet,
                        nume_d4612 = ped.nume,
                        localitate_d4612 = ped.localitate,
                        data_impr_d4612 = dataimp_d4612,
                        val_impr_d4612 = ped.acimp,
                        rest_impr_d4612 = ped.restimp,
                        sold_impr_d4612 = ped.soldimp - ped.debit_imprumut,
                        lunaimp_impr_d4612 = pens_d4612.lunaimp.ToString("dd.MM.yyyy"),
                        debit_impr_d4612 = ped.debit_imprumut
                    });
                }

           

            foreach (D_4612 d4612l in D4612_list_ordonat)
            {

                Cell nrcl = new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                       .Add(new Paragraph(d4612l.nrcarnet_d4612.ToString()).AddStyle(normalTbl));
                table.AddCell(nrcl);

                Cell numel = new Cell(1, 1)
                       .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                        .Add(new Paragraph(d4612l.nume_d4612).AddStyle(normalTbl));
                table.AddCell(numel);

                Cell locl = new Cell(1, 1)
                        .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                         .Add(new Paragraph(d4612l.localitate_d4612).AddStyle(normalTbl));
                table.AddCell(locl);


                Cell dataimpl = new Cell(1, 1)
                      .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                       .Add(new Paragraph(d4612l.data_impr_d4612.ToString("dd.MM.yyyy")).AddStyle(normalTbl));
                table.AddCell(dataimpl);

                Cell valimpl = new Cell(1, 1)
                     .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                      .Add(new Paragraph(d4612l.val_impr_d4612.ToString("N2")).AddStyle(normalTbl));
                table.AddCell(valimpl);


                Cell soldimpl = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph(d4612l.sold_impr_d4612.ToString("N2")).AddStyle(normalTbl));
                if (nrcol==8) table.AddCell(soldimpl);


                Cell lunaimpl = new Cell(1, 1)
                   .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                    .Add(new Paragraph(d4612l.lunaimp_impr_d4612).AddStyle(normalTbl));
                if (nrcol == 8) table.AddCell(lunaimpl);

                Cell debitl = new Cell(1, 1)
                    .SetTextAlignment(TextAlignment.CENTER).SetVerticalAlignment(VerticalAlignment.MIDDLE)
                     .Add(new Paragraph(d4612l.debit_impr_d4612.ToString("N2")).AddStyle(normalTbl));
                table.AddCell(debitl);

            }

            total_d4612 = 0;
            total_d4612 = D4612_list_ordonat.Sum(d4612s => d4612s.debit_impr_d4612);
            ViewData["d4612T"] = total_d4612.ToString("N2");
            Paragraph d4612T = new Paragraph();
            d4612T.Add(new Text("Total debit ").SetTextAlignment(TextAlignment.LEFT)
              .SetFontSize(13).AddStyle(normal).SetBold());
            d4612T.Add(new Text(total_d4612.ToString("N2")).SetTextAlignment(TextAlignment.LEFT)
             .SetFontSize(13).AddStyle(normal));
            document.Add(d4612T);
            document.Add(new Paragraph(" "));

            document.Add(table);
            table.Complete();





            document.Add(new Paragraph(" "));

            int i = 1;
            for (i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                float x = 20f;
                float y = pageSize.GetTop() - 20;
                document.ShowTextAligned(header1, x, y, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
                document.ShowTextAligned(header2, x, y - 20f, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);
            }


            int numberOfPages = pdfDoc.GetNumberOfPages();

            for (i = 1; i <= pdfDoc.GetNumberOfPages(); i++)
            {
                Rectangle pageSize = pdfDoc.GetPage(i).GetPageSize();
                float y = 20f;
                float x = pageSize.GetBottom() + 20;
                document.ShowTextAligned(new Paragraph(DateTime.Now.ToString("dd.MM.yyyy") + "                                                                                                                                                                                      " + "pagina " + i + " din " + numberOfPages).AddStyle(normal), x, y, i, TextAlignment.LEFT, VerticalAlignment.BOTTOM, 0);

            }

            titlu_raport_pens = "Situatie debitori pentru luna " + luna_d4612_in;


            TableHeaderEventHandler handler = new TableHeaderEventHandler(document);
            pdfDoc.AddEventHandler(PdfDocumentEvent.END_PAGE, handler);

            document.Close();
                                  

            ViewData["titlud4612"] = titlu_raport_pens;
            ViewData["titlud4612T"] = "Total situatie debitori pentru luna " + luna_d4612_in;


            ViewData["luna"] = luna_d4612_in;
            ViewData["lunal"] = luna_d4612;

            ViewData["nrcol"] =nrcol;


            var pageNumber = page ?? 1;
            ViewData["d4612"] = D4612_list_ordonat.ToList().ToPagedList(pageNumber, 15);

            decimal sum = D4612_list.Sum(sd => sd.debit_impr_d4612);
            ViewData["d4612sum"] = sum;
            return View(ViewData["d4612"]);

        }


        public async Task<ActionResult> listare_notificari_debitori_4612(int luna_d4612_inl = 0, int? page = 1)
        {
            //cek 10 nr 23 pe 1 pag 3 mijloc 3 end, nr record
          

            int luna_d4612 = 0;
            int an_d4612 = DateTime.Now.Year;
            string nrcarnet_d4612_str;
            int nrcarnet_d4612_int;
            int id4612 = 0;
            string pdfd4612 = "";
            string pdfd4612n = "";
            decimal trei_rate = 0;
            DateTime dataimp_d4612;
            decimal debit_4612 = 0;

            if (luna_d4612_inl == 12) an_d4612 = DateTime.Now.Year - 1;

            luna_d4612 = luna_d4612_inl;
            
            //declare var database
            var pens_d4612l = await _context.Pensionars.Where(pd4612 => pd4612.nrcarnet == 3893).SingleOrDefaultAsync();
            var rj_D4612l = await _context.registru_jurnal.Where(rjd4612 => (rjd4612.data.Month == luna_d4612) && (rjd4612.id_CS_debitor == 107) && (rjd4612.data.Year == an_d4612)).OrderBy(rjd4612 => rjd4612.id_CA_debitor).ToListAsync();
            var D4612_listl = new List<D_4612l>();
            var D4612_list_ordonatl = D4612_listl.OrderBy(d4612l => d4612l.nrcarnet_d4612l);

            //prg
            var pens_end_desfl = await _context.Pensionars.Where(ped => (ped.desfasurator > 0) && (ped.desfasurator <= luna_d4612)).ToListAsync();

            MemoryStream ms = new MemoryStream();
            PdfWriter writer = new PdfWriter(ms);
            PdfDocument pdfDoc = new PdfDocument(writer);
            Document document = new Document(pdfDoc, iText.Kernel.Geom.PageSize.A4, false);
            writer.SetCloseStream(false);
            document.SetMargins(100f, 20f, 50f, 20f);           


            foreach (registru_jurnal rjd4612 in rj_D4612l)
            {
                nrcarnet_d4612_str = rjd4612.id_CA_debitor.ToString();
                nrcarnet_d4612_str = nrcarnet_d4612_str.Remove(0, 4);
                nrcarnet_d4612_int = Int32.Parse(nrcarnet_d4612_str.Trim());
                pens_d4612l = await _context.Pensionars.Where(pd4612 => pd4612.nrcarnet == nrcarnet_d4612_int).SingleOrDefaultAsync();

                trei_rate = 0;
                if (pens_d4612l.acimp > 10000) trei_rate = pens_d4612l.acimp / 60;
                if (pens_d4612l.acimp <= 10000) trei_rate = pens_d4612l.acimp / 36;
                if (pens_d4612l.acimp <= 3000) trei_rate = pens_d4612l.acimp / 24;

                id4612 = 1;
                id4612 = id4612 + 1;
                dataimp_d4612 = (DateTime)pens_d4612l.dataimp;

                debit_4612 = rjd4612.SF_CA_debit;                
                if (3 * trei_rate < debit_4612)
                    if ((pens_d4612l.soldimp != 0) || (pens_d4612l.debit_imprumut != 0))
                        D4612_listl.Add(new D_4612l
                        {
                            id_d4612l = id4612,
                            nrcarnet_d4612l = nrcarnet_d4612_int,
                            nume_d4612l = pens_d4612l.nume,
                            judet_d4612l = pens_d4612l.judet,
                            localitate_d4612l = pens_d4612l.localitate,                            
                            strada_d4612l = pens_d4612l.strada,
                            bloc_d4612l = pens_d4612l.bloc,
                            nr_d4612l = pens_d4612l.nr,
                            ap_d4612l = pens_d4612l.ap

                        });
            }

            if (luna_d4612_inl != 0)
                foreach (Pensionar ped in pens_end_desfl)
                {

                    id4612 = 1;
                    id4612 = id4612 + 1;
                    dataimp_d4612 = (DateTime)ped.dataimp;

                    D4612_listl.Add(new D_4612l
                    {
                        id_d4612l = id4612,
                        nrcarnet_d4612l = ped.nrcarnet,
                        nume_d4612l = ped.nume,
                        judet_d4612l = ped.judet,
                        localitate_d4612l = ped.localitate,
                        strada_d4612l = ped.strada,
                        bloc_d4612l = ped.bloc,
                        nr_d4612l = ped.nr,
                        ap_d4612l = ped.ap

                    });
                }


            Style normalCL = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            normalCL.SetFont(font).SetFontSize(12);

            foreach (D_4612l d4612l in D4612_list_ordonatl)
            {
                Paragraph p1CL = new Paragraph("C.A.R. Pensionari Turnu Magurele")
                      .SetTextAlignment(TextAlignment.LEFT)
                      .SetBold().AddStyle(normalCL);
                document.Add(p1CL);

                Paragraph p2CL = new Paragraph("Nr .............. din ..............")
                    .SetTextAlignment(TextAlignment.LEFT)
                    .AddStyle(normalCL);
                document.Add(p2CL);

                Paragraph spatiuCL = new Paragraph("");
                document.Add(spatiuCL);
                document.Add(spatiuCL);

                Paragraph p3CL = new Paragraph("CATRE DEBITORUL")
                   .SetTextAlignment(TextAlignment.LEFT)
                   .SetBold().AddStyle(normalCL);
                document.Add(p3CL);

                document.Add(spatiuCL);

                Paragraph pnumenrc = new Paragraph("Nume: " + d4612l.nume_d4612l.Trim() + ", " + "nr carnet: " + d4612l.nrcarnet_d4612l.ToString())
                   .SetTextAlignment(TextAlignment.LEFT)
                   .AddStyle(normalCL);
                document.Add(pnumenrc);

                Paragraph pJL = new Paragraph("judet: " + d4612l.judet_d4612l.Trim() + ", " + "localitate: " + d4612l.localitate_d4612l.Trim())
                    .SetTextAlignment(TextAlignment.LEFT)
                  .AddStyle(normalCL);
                document.Add(pJL);


                Paragraph padr = new Paragraph("strada: " + d4612l.strada_d4612l.Trim() + ", " + "nr: " + d4612l.nr_d4612l.Trim() + ", " + "bloc: " + d4612l.bloc_d4612l.Trim() + ", " + "ap: " + d4612l.ap_d4612l.Trim())
                   .SetTextAlignment(TextAlignment.LEFT)
                  .AddStyle(normalCL);
                document.Add(padr);

                document.Add(spatiuCL);

                Paragraph pnotif = new Paragraph("Va informam prin prezenta ca, in termen de zece zile de la primirea prezentei adrese, sa va prezentati la casieria unitatii pentru plata cotizatiilor restante. In caz contrar veti fi exclusi din randurile membrilor")
                   .SetTextAlignment(TextAlignment.LEFT)
                  .AddStyle(normalCL);
                document.Add(pnotif);

                document.Add(spatiuCL);
                document.Add(spatiuCL);

                Paragraph pSIGN = new Paragraph("PRESEDINTE                                                                                       CONTABIL SEF")
                   .SetTextAlignment(TextAlignment.LEFT).SetBold()
                  .AddStyle(normalCL);
                document.Add(pSIGN);
                document.Add(spatiuCL);
                document.Add(spatiuCL);
                document.Add(spatiuCL);
                document.Add(spatiuCL);
                document.Add(spatiuCL);
                document.Add(spatiuCL);
                document.Add(spatiuCL);
                document.Add(spatiuCL);
                document.Add(spatiuCL);
               
               

            }

           
            document.Close();
            byte[] byteInfo = ms.ToArray();
            ms.Write(byteInfo, 0, byteInfo.Length);
            ms.Position = 0;

            FileStreamResult fileStreamResult = new FileStreamResult(ms, "application/pdf");
           
            return fileStreamResult;

        }

        public ActionResult introducere_credit_restantieri()
        {
            //DE TESTAT BEFORE RUN



            //I  SELECT* from Pensionars where soldimp > 0 and credit_imprumut> 0 and desfasurator = 0 and nrcarnet not in (select nrcarnet from desfasurator_rate )

            //II SELECT* from Pensionars where soldimp > 0 and credit_imprumut> 0 and desfasurator = 0 and nrcarnet not in (select nrcarnet from desfasurator_rate where month(data_rata) = 8  and year(data_rata)= 2023)


            int contor_ch = 0;
            int chidmax_ins_cr_rstnt = 0;
            decimal rata_de_plata_ins_cr_rest = 0, credit_use_ins_restntier = 0;
            int idcredit = 0;
            int luna = 8;
            DateTime data_ins;
            int nr_NC = 10;

         


            //INITIALIZARI
            luna = DateTime.Now.Month;
            //data_ins = Convert.ToDateTime("2024-09-30 18:00:00");
            data_ins = DateTime.Now;
            nr_NC = 10;
            //END IN INITIALIZARI


            



            var pens_ins_credit_restantieri = _context.Pensionars
                .Where(picr => ((picr.nrcarnet < 20000) && (picr.soldimp > 0) && (picr.credit_imprumut > 0) && (picr.desfasurator == 0))).OrderBy(picr => picr.nrcarnet).ToList();

            var desf_ins_crdt_rstnt = _context.desfasurator_rate.Where(dicr => ((dicr.nr_rata == 1) && (dicr.nrcarnet == 3893))).SingleOrDefault();

            //verificari a mai fost inchisa luna
            //
            int zicheck = 0;
           
            //
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 6)) zicheck = 30;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 7)) zicheck = 31;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 8)) zicheck = 29;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 9)) zicheck = 30;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 10)) zicheck = 31;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 11)) zicheck = 28;

            if (DateTime.Now.Day!=zicheck)
            {
                ViewData["Error"] = "Nu se poate face inchidere de luna in aceasta zi";
                return View("Error");
            }
            var cekEndLuna_rj = _context.registru_jurnal.Where(celrj => (celrj.data.Day ==zicheck) && 
            (celrj.data.Month == luna) && (celrj.data.Year == DateTime.Now.Year) && (celrj.nr_document== "10") && (celrj.id_CS_debitor==106));
            if (cekEndLuna_rj.Count() >0)
            {
                ViewData["Error"] = "Au mai fost introduse NC pentru  sfarsit de luna pentru credit";
                return View("Error");
            }
            //END VERIFICARI A MAI FOST CLOSE LUNA



            int i = 0;
            foreach (Pensionar picr in pens_ins_credit_restantieri)
            {

                chidmax_ins_cr_rstnt = _context.Chitantas.Max(c => c.idchitanta);

                //   

                contor_ch = _context.Chitantas
                    .Where(cicr => ((cicr.data.Month == luna) && (cicr.data.Year == DateTime.Now.Year) && (cicr.nrcarnet == picr.nrcarnet) && (cicr.rata > 0))).Count();


                desf_ins_crdt_rstnt = _context.desfasurator_rate
                    .Where(dr => (dr.nrcarnet == picr.nrcarnet) && (dr.data_rata.Month == luna) && (dr.data_rata.Year == DateTime.Now.Year)).SingleOrDefault();

                rata_de_plata_ins_cr_rest = desf_ins_crdt_rstnt.rata_de_plata;


                if (contor_ch == 0)
                {
                    i = i + 1;

                    if (picr.credit_imprumut >= rata_de_plata_ins_cr_rest) credit_use_ins_restntier = rata_de_plata_ins_cr_rest;

                    if (picr.credit_imprumut < rata_de_plata_ins_cr_rest) credit_use_ins_restntier = picr.credit_imprumut;

                    //de aici ian 2024
                    //if (iesire_462 != 0)
                    //{
                 



                    _context.registru_jurnal.Add(new registru_jurnal
                    {
                        data = data_ins,
                        id_document = chidmax_ins_cr_rstnt + 1,
                        nr_document = nr_NC.ToString(),
                        id_CS_debitor = 106,
                        id_CA_debitor = Int32.Parse("4622" + picr.nrcarnet.ToString()),
                        id_CS_creditor = 103,
                        id_CA_creditor = Int32.Parse("2678" + picr.nrcarnet.ToString()),
                        explicatie_nr_cont_analitic = "-",
                        SI_CS_debit = 0,
                        SI_CA_debit = picr.credit_imprumut,
                        SI_CS_credit = 0,
                        SI_CA_credit = picr.soldimp,
                        debit = credit_use_ins_restntier,
                        credit = credit_use_ins_restntier,
                        SF_CS_debit = 0,
                        SF_CA_debit = picr.credit_imprumut - credit_use_ins_restntier,
                        SF_CS_credit = 0,
                        SF_CA_credit = picr.soldimp - credit_use_ins_restntier,
                        tip_document = 11,
                        sortare = "H"
                    });

                    picr.credit_imprumut = picr.credit_imprumut - credit_use_ins_restntier;
                    picr.soldimp = picr.soldimp - credit_use_ins_restntier;
                    picr.restimp = picr.restimp + credit_use_ins_restntier;


                    desf_ins_crdt_rstnt.rata_platita = credit_use_ins_restntier;
                    desf_ins_crdt_rstnt.rata_platita_initial = credit_use_ins_restntier;

                    picr.soldtaxainscr = DateTime.Now.Month;//1 luna cinci c de la credit

                    _context.SaveChanges();

                    //}

                    //
                    _context.Chitantas.Add(new Chitanta
                    {
                        nrch = nr_NC,
                        data = data_ins,
                        nrcarnet = picr.nrcarnet,
                        cotizatie = 0,
                        ajdeces = 0,
                        lunaajdeces = picr.lunaajdeces,
                        rata_de_plata = desf_ins_crdt_rstnt.rata_de_plata,
                        rata = 0,
                        dobanda = 0,
                        credit_imprumut = credit_use_ins_restntier,
                        taxainscr = 0,
                        total = credit_use_ins_restntier,
                        nume = picr.nume,
                        tip_operatie = "NC",
                        tip_document = 11,
                        serie= "-"
                    });

                    _context.SaveChanges();

                    //pa code

                    //debit
                    idcredit = Int32.Parse("4622" + picr.nrcarnet.ToString());

                    var checkIDcredit = _context.conturi_analitice
                        .Where(cic => cic.id_cont_analitic == idcredit);

                    if (checkIDcredit.Count() == 0)
                    {
                        _context.conturi_analitice.Add(new conturi_analitice
                        {
                            id_cont_analitic = Int32.Parse("4622" + picr.nrcarnet.ToString()),
                            id_cont_sintetic = 106,
                            nr_cont_analitic = "4622." + picr.nrcarnet.ToString(),
                            explicatie_nr_cont_analitic = picr.nume,
                        });
                        _context.SaveChanges();
                    }


                }



            }

            ViewBag.nrins = i;
            ViewBag.nrins = "A fost introdus credit pentru un numar de " + i + " partide";


            return View();
        }

        public ActionResult introducere_debit_restantieri()
        {

            //DE TESTAT BEFORE RUN
            //I SELECT* from pensionars where  soldimp > 0 and nrcarnet not in (select nrcarnet from desfasurator_rate where month(data_rata) = 4 and year(data_rata)= 2023) order by dataimp

            int idchmax = 0;
            int iddebit = 0;
            int luna = 0;
            DateTime data_ins;
            int nr_NC = 0;

            //initializare
            luna = DateTime.Now.Month;
            // data_ins = Convert.ToDateTime("2024-9-30 19:00:00");
            data_ins = DateTime.Now;
            nr_NC = 10;

            //end initializare -  nu linia de jos lunaimp

            var py = _context.Pensionars
              //   .Where(p_y => (p_y.nrcarnet<20000) && (p_y.soldimp > 0) && (p_y.lunaimp.Month <10) && (p_y.lunaimp.Year == 2024) && (p_y.desfasurator == 0)).OrderBy(p_y => p_y.nrcarnet).ToList();
              .Where(p_y => (p_y.nrcarnet < 20000) && (p_y.soldimp > 0)
                   && (((p_y.lunaimp.Month < DateTime.Now.Month) && (p_y.lunaimp.Year == DateTime.Now.Year)) || (p_y.lunaimp.Year < DateTime.Now.Year)) && (p_y.desfasurator == 0)).OrderBy(p_y => p_y.nrcarnet).ToList();


            //verificari a mai fost inchisa luna
            //
            int zicheck = 0;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 6)) zicheck = 30;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 7)) zicheck = 31;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 8)) zicheck = 29;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 9)) zicheck = 30;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 10)) zicheck = 31;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 11)) zicheck = 28;

            if (DateTime.Now.Day != zicheck)
            {
                ViewData["Error"] = "Nu se poate face inchidere de luna in aceasta zi";
                return View("Error");
            }
            var cekEndLuna_rj = _context.registru_jurnal.Where(celrj => (celrj.data.Day == zicheck) &&
            (celrj.data.Month == luna) && (celrj.data.Year == DateTime.Now.Year) && (celrj.nr_document == "10") && (celrj.id_CS_debitor == 106));
            if (cekEndLuna_rj.Count() < 60)
            {
                ViewData["Error"] = "Nu au fost introduse NC pentru  sfarsit de luna pentru credit";
                return View("Error");
            }
            //END VERIFICARI A MAI FOST CLOSE LUNA
            cekEndLuna_rj = _context.registru_jurnal.Where(celrj => (celrj.data.Day == zicheck) &&
            (celrj.data.Month == luna) && (celrj.data.Year == DateTime.Now.Year) && (celrj.nr_document == "10") && (celrj.id_CS_debitor == 107));
            if (cekEndLuna_rj.Count() >1)
            {
                ViewData["Error"] = "Au mai fost introduse NC pentru  sfarsit de luna pentru debit";
                return View("Error");
            }




            int testf = 0;
            int i = 0;
            foreach (Pensionar yyy in py)

            {


                testf = 0;
                idchmax = _context.Chitantas.Max(c => c.idchitanta);
                idchmax = idchmax + 1;


                var chy = _context.Chitantas
                          .Where(ch_y => (ch_y.nrcarnet == yyy.nrcarnet) && (ch_y.rata > 0) && (ch_y.data.Month == luna));
                if (chy.Count() == 0)
                {

                    var dy = _context.desfasurator_rate
                        .Where(d_y => (d_y.nrcarnet == yyy.nrcarnet) && (d_y.data_rata.Month == luna) && (d_y.data_rata.Year == DateTime.Now.Year)).SingleOrDefault();

                    if (dy.rata_de_plata > dy.rata_platita)
                    {
                        i = i + 1;
                        var exista_c = _context.Chitantas.Where(ec => (ec.nrcarnet == yyy.nrcarnet) && (ec.nrch == nr_NC) && (ec.data.Month == luna)).SingleOrDefault();

                        if (exista_c != null) idchmax = exista_c.idchitanta;


                        //pa
                        _context.registru_casa.Add(new registru_casa
                        {
                            data = data_ins,
                            nr_document = nr_NC.ToString(),
                            id_chimpr = idchmax,
                            id_cont_sintetic = 107,
                            id_cont_analitic = Int32.Parse("4612" + yyy.nrcarnet.ToString()),
                            explicatie_nr_cont_analitic = "-",
                            sold_initial = yyy.debit_imprumut,
                            suma = dy.rata_de_plata - dy.rata_platita,
                            intrare = dy.rata_de_plata - dy.rata_platita,
                            incasare = 0,
                            plata = 0,

                            sold_final = yyy.debit_imprumut + dy.rata_de_plata - dy.rata_platita,
                            tip = "intrare",
                            sortare = "H"
                          

                        });



                        //de aici
                        //    if (intrare_461 != 0)
                        //  {

                        _context.registru_jurnal.Add(new registru_jurnal
                        {
                            data = data_ins,
                            id_document = idchmax,
                            nr_document = nr_NC.ToString(),
                            id_CS_debitor = 107,
                            id_CA_debitor = Int32.Parse("4612" + yyy.nrcarnet.ToString()),
                            id_CS_creditor = 103,
                            id_CA_creditor = Int32.Parse("2678" + yyy.nrcarnet.ToString()),
                            explicatie_nr_cont_analitic = "-",
                            SI_CS_debit = 0,
                            SI_CA_debit = yyy.debit_imprumut,
                            SI_CS_credit = 0,
                            SI_CA_credit = yyy.soldimp,
                            debit = dy.rata_de_plata - dy.rata_platita,
                            credit = dy.rata_de_plata - dy.rata_platita,
                            SF_CS_debit = 0,
                            SF_CA_debit = yyy.debit_imprumut + dy.rata_de_plata - dy.rata_platita,
                            SF_CS_credit = 0,
                            SF_CA_credit = yyy.soldimp,
                            tip_document = 11,
                            sortare = "H"
                        });

                        yyy.debit_imprumut = yyy.debit_imprumut + dy.rata_de_plata - dy.rata_platita;
                        yyy.soldtaxainscr = 82;  //3 de la luna doi de la debit
                        _context.SaveChanges();


                        //de aici

                        if (exista_c != null)
                        {
                            exista_c.debit_imprumut = dy.rata_de_plata - dy.rata_platita;
                            exista_c.total = exista_c.total + dy.rata_de_plata - dy.rata_platita;
                            _context.SaveChanges();
                        }

                        if (exista_c == null)
                            //
                            _context.Chitantas.Add(new Chitanta
                            {
                                nrch = nr_NC,
                                data = data_ins,
                                nrcarnet = yyy.nrcarnet,
                                cotizatie = 0,
                                ajdeces = 0,
                                lunaajdeces = yyy.lunaajdeces,
                                rata_de_plata = dy.rata_de_plata,
                                rata = 0,
                                dobanda = 0,
                                debit_imprumut = dy.rata_de_plata - dy.rata_platita,
                                taxainscr = 0,
                                total = dy.rata_de_plata - dy.rata_platita,
                                nume = yyy.nume,
                                tip_document = 11,
                                tip_operatie = "NC",
                                serie = "-"
                            });

                        _context.SaveChanges();

                        //pa code



                        //debit
                        iddebit = Int32.Parse("4612" + yyy.nrcarnet.ToString());

                        var checkIdDebit = _context.conturi_analitice
                            .Where(cic => cic.id_cont_analitic == iddebit);

                        if (checkIdDebit.Count() == 0)
                        {
                            _context.conturi_analitice.Add(new conturi_analitice
                            {
                                id_cont_analitic = Int32.Parse("4612" + yyy.nrcarnet.ToString()),
                                id_cont_sintetic = 107,
                                nr_cont_analitic = "4612." + yyy.nrcarnet.ToString(),
                                explicatie_nr_cont_analitic = yyy.nume,
                            });
                            _context.SaveChanges();
                        }


                    }
                }

            }

            ViewBag.nrinsd = "A fost introdus debit pentru un numar de " + i + " partide";
            return View();
        }



        public ActionResult calcul_DP_end_luna()
        {
            //cod bun 04 aprilie 2025
            //cod RUN 2 IULIE 2025  a fost ok si cel cod bun 04 aprilie 2025 

            int zicheck = 0;
            int luna = DateTime.Now.Month;

            Decimal treiRP = 0;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 3)) zicheck = 31;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 4)) zicheck = 30;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 5)) zicheck = 30;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 6)) zicheck = 30;


            if (DateTime.Now.Day != zicheck)
            {
                ViewData["Error"] = "Nu se poate face inchidere de luna in aceasta zi";
                return View("Error");
            }
            var cekEndLuna_rj = _context.registru_jurnal.Where(celrj => (celrj.data.Day == zicheck) &&
            (celrj.data.Month == luna) && (celrj.data.Year == DateTime.Now.Year) && (celrj.nr_document == "10") && (celrj.id_CS_debitor == 106));
            if (cekEndLuna_rj.Count() < 60)
            {
                return View("Error");
            }




            //30 ian 2024
            //pt referinta luna septembrie ex
            // DateTime date3RP = Convert.ToDateTime("2023-9-1");
            // Decimal treiRP = 0;
            DateTime Dreferinta = Convert.ToDateTime("2020-1-1");
            //DateTime DreferintaEnd = Convert.ToDateTime("2023-9-1");

            //DateTime Ddebit6Start = Convert.ToDateTime("2023-3-1 00:00:00");
            //DateTime Ddebit6End = Convert.ToDateTime("2023-10-1 00:00:00");

            //INITIALIZARI                      

            //DateTime date3RP1 = Convert.ToDateTime("2025-2-1");


            // Decimal treiRP = 0;
            // DateTime Dreferinta = Convert.ToDateTime("2020-1-1");
            //   DateTime DreferintaEnd = Convert.ToDateTime("2025-2-1");

            //    DateTime Ddebit6Start = Convert.ToDateTime("2024-8-1 00:00:00");
            //    DateTime Ddebit6End = Convert.ToDateTime("2025-3-1 00:00:00");

            //   DateTime luna_ins_dp = Convert.ToDateTime("2025-2-1 00:00:00");




            DateTime date3RP = Convert.ToDateTime(DateTime.Now.Year.ToString() + "-" +
                      DateTime.Now.Month.ToString() + "-1" + " " + "00:00:00");

            DateTime DreferintaEnd = Convert.ToDateTime(DateTime.Now.Year.ToString() + "-" +
                      DateTime.Now.Month.ToString() + "-1" + " " + "00:00:00");

            DateTime Ddebit6Start = DateTime.Now.AddMonths(-6);
            Ddebit6Start = Convert.ToDateTime(Ddebit6Start.Year.ToString() + "-" +
                      Ddebit6Start.Month.ToString() + "-1" + " " + "00:00:00");

            DateTime Ddebit6End = DateTime.Now.AddMonths(1);
            Ddebit6End = Convert.ToDateTime(Ddebit6End.Year.ToString() + "-" +
                      Ddebit6End.Month.ToString() + "-1" + " " + "00:00:00");

            DateTime luna_ins_dp = Convert.ToDateTime(DateTime.Now.Year.ToString() + "-" +
                     DateTime.Now.Month.ToString() + "-1" + " " + "00:00:00");


            _context.Add(new cekDPInit
            {
                date3RP1 = date3RP,
                DreferintaEnd1 = DreferintaEnd,
                Ddebit6Start1 = Ddebit6Start,
                Ddebit6End1 = Ddebit6End,
                luna_ins_dp1 = luna_ins_dp
            });

            _context.SaveChanges();

            int sf0_luna = DateTime.Now.Month;
            //int sf0_luna = 6;
            //martie dp 2025
            //END INITIALIZARI - MAI AM DE INITIALIZAT IN FOREACH DEBITSTART6

            decimal rest = 0;
            int test3 = 0;
            int id_ca = 0;
            int test_sf0 = 0;

            var pens_clcDPEndLuna = _context.Pensionars.Where(cdpel => cdpel.soldimp > 0).OrderBy(cdpel => cdpel.nrcarnet).ToList();
            var rj_cdp = _context.registru_jurnal.Where(rjcdp => (rjcdp.id_CA_debitor == 461240) && (rjcdp.data.Month == 3) && (rjcdp.data.Year == 2025)).OrderByDescending(rjcdp => rjcdp.data).ToList();


            foreach (Pensionar pcdpel in pens_clcDPEndLuna)
            {
                test3 = 0;
                treiRP = 0;
                rest = 0;
                Dreferinta = pcdpel.luna_DP;
                decimal debit_dp = 0;
                DateTime data_luna_calcul_dp;
                int nr_zile_luna_DP = 0;
                decimal dobanda_penalizatoare = 0;

                test_sf0 = 0;


                //Ddebit6Start = Convert.ToDateTime("2024-12-1 00:00:00");
                Ddebit6Start = DateTime.Now.AddMonths(-6);
                Ddebit6Start = Convert.ToDateTime(Ddebit6Start.Year.ToString() + "-" +
                          Ddebit6Start.Month.ToString() + "-1" + " " + "00:00:00");


                var get3RP = _context.desfasurator_rate.Where(g3rp => (g3rp.nrcarnet == pcdpel.nrcarnet) && (g3rp.data_rata < date3RP)).OrderByDescending(g3rp => g3rp.data_rata).Take(3);
                if (get3RP.Count() >= 3) treiRP = get3RP.Sum(g3rp => g3rp.rata_de_plata);

                if (get3RP.Count() < 3) test3 = 1;


                if (test3 == 0)
                {

                    var getRest = _context.desfasurator_rate.Where(gr => (gr.nrcarnet == pcdpel.nrcarnet) && (gr.data_rata < Dreferinta) && (gr.rata_de_plata > gr.rata_platita));

                    if (getRest.Count() >= 1)
                        rest = getRest.Sum(gr => gr.rata_de_plata - gr.rata_platita);


                    var DesfgetDP = _context.desfasurator_rate.Where(dgdp => (dgdp.nrcarnet == pcdpel.nrcarnet) && (dgdp.data_rata > Dreferinta) && (dgdp.data_rata < DreferintaEnd) && (dgdp.rata_de_plata > dgdp.rata_platita));

                    debit_dp = rest;
                    foreach (desfasurator_rate dgdp in DesfgetDP)
                    {
                        nr_zile_luna_DP = 0;

                        data_luna_calcul_dp = dgdp.data_rata.AddMonths(1);
                        nr_zile_luna_DP = DateTime.DaysInMonth(data_luna_calcul_dp.Year, data_luna_calcul_dp.Month);
                        debit_dp = debit_dp + dgdp.rata_de_plata - dgdp.rata_platita;
                        dobanda_penalizatoare = dobanda_penalizatoare + 0.01M * nr_zile_luna_DP * debit_dp * 0.01M;
                    }

                    if (debit_dp < treiRP)
                    {
                        id_ca = Int32.Parse("4612" + pcdpel.nrcarnet.ToString());
                        if (Dreferinta > Ddebit6Start) Ddebit6Start = Dreferinta;
                        //var get6Debit = _context.registru_casa.Where(g6d => (g6d.id_cont_analitic == id_ca) && (g6d.data > Ddebit6Start) && (g6d.data < Ddebit6End) && (g6d.id_cont_sintetic == 107) && (g6d.tip == "intrare")).ToList();
                        var get6Debit = _context.registru_jurnal.Where(g6d => (g6d.id_CA_debitor == id_ca) && (g6d.data > Ddebit6Start) && (g6d.data < Ddebit6End)).ToList();
                        if (get6Debit.Count() < 6) dobanda_penalizatoare = 0;

                        if (get6Debit.Count() >= 6)
                        {
                            dobanda_penalizatoare = 0;

                            debit_dp = rest;
                            foreach (registru_jurnal g6d in get6Debit)
                            {
                                test_sf0 = 0;

                                nr_zile_luna_DP = 0;
                                data_luna_calcul_dp = g6d.data;
                                nr_zile_luna_DP = DateTime.DaysInMonth(data_luna_calcul_dp.Year, data_luna_calcul_dp.Month);
                                //debit_dp = rest + g6d.sold_final - g6d.intrare;
                                debit_dp = rest + g6d.SF_CA_debit - g6d.credit;
                                dobanda_penalizatoare = dobanda_penalizatoare + 0.01M * nr_zile_luna_DP * debit_dp * 0.01M;


                                //de aici
                                if ((g6d.SI_CA_debit == 0) && (g6d.data.Month == sf0_luna) && (g6d.data.Year == 2025))
                                {
                                    dobanda_penalizatoare = 0;
                                    test_sf0 = 1;
                                }
                                //pa

                                //pa cek
                                //rj_cdp = _context.registru_jurnal.Where(rjcdp => (rjcdp.id_CA_creditor == g6d.id_cont_analitic) && (rjcdp.data.Month == DateTime.Now.Month) && (rjcdp.data.Year == DateTime.Now.Year)).OrderByDescending(rjcdp => rjcdp.data).ToList();
                                rj_cdp = _context.registru_jurnal.Where(rjcdp => (rjcdp.id_CA_creditor == g6d.id_CA_debitor) && (rjcdp.data.Month == DateTime.Now.Month) && (rjcdp.data.Year == DateTime.Now.Year)).OrderByDescending(rjcdp => rjcdp.data).ToList();
                                if ((rj_cdp.Count > 0) && (rj_cdp.First().SF_CA_credit == 0) && (test_sf0 == 0))
                                {
                                    dobanda_penalizatoare = 0;
                                    pcdpel.soldtaxainscr = 725;
                                    _context.SaveChanges();

                                }
                                //cek 721 if pune 0 si ? 633
                            }

                        }


                        treiRP = 6;
                    }



                    //?trebuie?
                    // if (debit_dp < treiRP) dobanda_penalizatoare = 0;


                    if (dobanda_penalizatoare != 0)
                    {

                        _context.dp_lunar.Add(new dp_lunar
                        {
                            nrcarnet = pcdpel.nrcarnet,
                            dp = dobanda_penalizatoare,
                            luna = luna_ins_dp,
                            trei = treiRP
                        });

                        pcdpel.luna_DP = luna_ins_dp;
                        //     pcdpel.sold_DP = pcdpel.sold_DP + dobanda_penalizatoare;

                        //  if (test_sf0 == 1) pcdpel.soldtaxainscr = 633;

                        _context.SaveChanges();
                    }

                    if ((test_sf0 == 1) && (dobanda_penalizatoare == 0))
                    {
                        pcdpel.soldtaxainscr = 635;
                        _context.SaveChanges();
                    }
                }



            }


            //end 2022 clcdp
            return View();


        }



        public ActionResult insert_RJ_DP_end_luna()
        {
            int luna = 0;
            DateTime data_dp = Convert.ToDateTime("2025-5-30 23:00:00");

            //initializari
             luna = DateTime.Now.Month;
            
            // data_dp= Convert.ToDateTime("2024-9-30 20:00:00");
            data_dp = DateTime.Now;
            //end initializari
            int iddp = 0;


            int zicheck = 0;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 6)) zicheck = 30;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 7)) zicheck = 31;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 8)) zicheck = 29;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 9)) zicheck = 30;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 10)) zicheck = 31;
            if ((DateTime.Now.Year == 2025) && (DateTime.Now.Month == 11)) zicheck = 28;

            if (DateTime.Now.Day != zicheck)
            {
                ViewData["Error"] = "Nu se pot introduce dobanzi penalizatoare in aceasta zi";
                return View("Error");
            }



            var maiExista = _context.registru_jurnal.Where(me => (me.data.Day == DateTime.Now.Day) && (me.data.Month == DateTime.Now.Month) && (me.data.Year == DateTime.Now.Year)
                && (me.nr_document == "7") && (me.id_CS_debitor == 108)).ToList();

            if (maiExista.Count()>0)
            {
             ViewData["Error"] = "Au mai fost introduse dobanzile penalizatoare pentru aceasta luna";
             return View("Error");
            }



            var dp_luna = _context.dp_lunar.Where(dl => (dl.luna.Month == luna) && (dl.luna.Year == DateTime.Now.Year) && (dl.nrcarnet !=0) ).ToList();

            var pens_up_dp = _context.Pensionars.Where(pudp => pudp.nrcarnet == 3893).SingleOrDefault();

            var up_desf_dp = _context.desfasurator_rate.Where(udd => (udd.nrcarnet == 3893) && (udd.nr_rata == 1)).SingleOrDefault();
            var up_desf_dp_null = _context.desfasurator_rate.Where(uddn => (uddn.nrcarnet == 3893) && (uddn.nr_rata == 1) ).OrderByDescending(uddn=>uddn.data_rata).ToList();

            var chidmax_ins_cr_rstnt = _context.Chitantas.Max(c => c.idchitanta);

            foreach (dp_lunar dpl in dp_luna)
            {

                pens_up_dp = _context.Pensionars.Where(pudp => pudp.nrcarnet == dpl.nrcarnet).SingleOrDefault();

               

                _context.Chitantas.Add(new Chitanta
                {
                    nrch = '7',
                    data = data_dp,
                    nrcarnet = dpl.nrcarnet,
                    cotizatie = 0,
                    ajdeces = 0,
                    lunaajdeces = pens_up_dp.lunaajdeces,
                    rata_de_plata = 0,
                    rata = 0,
                    dobanda_penalizatoare=dpl.dp,
                    dobanda = 0,
                    credit_imprumut = 0,
                    taxainscr = 0,
                    total = dpl.dp,
                    nume = pens_up_dp.nume,
                    tip_operatie = "NC",
                    tip_document = 11,
                    serie= "-"
                });

                _context.SaveChanges();

                chidmax_ins_cr_rstnt = _context.Chitantas.Max(c => c.idchitanta);

                //de aici dec 2023
                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = data_dp,
                    id_document = chidmax_ins_cr_rstnt,
                    nr_document = "7",
                    id_CS_debitor = 108,
                    id_CA_debitor = Int32.Parse("6792" + dpl.nrcarnet.ToString()),
                    id_CS_creditor = 53,
                    id_CA_creditor = 103,
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = 0,
                    debit = dpl.dp,
                    credit = dpl.dp,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = 0,
                    tip_document = 11,
                    sortare = "H"
                });

                pens_up_dp.sold_DP = pens_up_dp.sold_DP + dpl.dp;
                _context.SaveChanges();

                up_desf_dp = _context.desfasurator_rate.Where(udd => (udd.nrcarnet == dpl.nrcarnet) && (udd.data_rata.Month == luna + 1) && (udd.data_rata.Year == DateTime.Now.Year)).SingleOrDefault();
                
                if (up_desf_dp !=null) up_desf_dp.dobanda_penalizatoare = dpl.dp;
                if (up_desf_dp==null)
                {
                 up_desf_dp_null=_context.desfasurator_rate.Where(udpn => (udpn.nrcarnet == dpl.nrcarnet)).OrderByDescending(udpn=>udpn.data_rata).ToList();
                 up_desf_dp_null.First().dobanda_penalizatoare = up_desf_dp_null.First().dobanda_penalizatoare + dpl.dp;
                 pens_up_dp.soldtaxainscr = 6263;
                }

                _context.SaveChanges();
                //cek aduna la old
                //cek pune all in desf 

                iddp = 0;
                iddp = Int32.Parse("6792" + pens_up_dp.nrcarnet.ToString());

                var checkIDdp = _context.conturi_analitice
                    .Where(cic => cic.id_cont_analitic == iddp);

                if (checkIDdp.Count() == 0)
                {
                    _context.conturi_analitice.Add(new conturi_analitice
                    {
                        id_cont_analitic = Int32.Parse("6792" + pens_up_dp.nrcarnet.ToString()),
                        id_cont_sintetic = 108,
                        nr_cont_analitic = "26792." + pens_up_dp.nrcarnet.ToString(),
                        explicatie_nr_cont_analitic = pens_up_dp.nume,
                    });
                    _context.SaveChanges();
                }


            }

            return View();
        }


        public ActionResult insert_dobanzi_4611P_end_luna()
        {
            int luna = 0;
            decimal getDobP = 0;
            decimal dobandaIns = 0;
            double dobandaInsD = 0;
            decimal dpIns = 0;
            int nr_zile_luna_DP_P = 0;
            DateTime data_dp = Convert.ToDateTime("2025-6-30 23:00:00");
            int chidmax_ins_cr_rstnt = 0;
            int nrcarnetP = 0;

            //initializari
            //luna = DateTime.Now.Month;

            // data_dp= Convert.ToDateTime("2024-9-30 20:00:00");
            // data_dp = DateTime.Now;
            
            //end initializari
            int iddp = 0;
                      

            var pens_ins_ddp_4611 = _context.Pensionars.Where(pid4611 => (pid4611.id_stare == 4) && (pid4611.sold_461 > 0)).ToList();
                                

            foreach (Pensionar pinsD4611 in pens_ins_ddp_4611)
            {                               

                if (pinsD4611.acimp <= 1000) getDobP = 0.05M;              

                if ((pinsD4611.acimp > 1000) && (pinsD4611.acimp <= 3000)) getDobP = 0.06M;               

                if ((pinsD4611.acimp > 3000) && (pinsD4611.acimp <= 10000)) getDobP = 0.075M;                

                if ((pinsD4611.acimp > 10000) && (pinsD4611.acimp <= 25000)) getDobP = 0.095M;                

                if ((pinsD4611.acimp > 25000) && (pinsD4611.acimp  <= 35000)) getDobP = 0.105M;             

                if ((pinsD4611.acimp > 35000) && (pinsD4611.acimp <= 49000))  getDobP = 0.115M;              

                if (pinsD4611.acimp == 70000)  getDobP = 0.12M;

                dobandaIns = 0;                
                dpIns = 0;
                dobandaIns = (pinsD4611.sold_461 * getDobP * 30) / 360;                
                dobandaIns =  round_value(dobandaIns);
                

                nr_zile_luna_DP_P = 0;
                //nr_zile_luna_DP_P = DateTime.DaysInMonth(DateTime.Now.Year, DateTime.Now.Month);                               
                nr_zile_luna_DP_P = DateTime.DaysInMonth(data_dp.Year, data_dp.Month);
                dpIns = 0.01M * nr_zile_luna_DP_P * pinsD4611.sold_461 * 0.01M;




                //   _context.Chitantas.Add(new Chitanta
                // {
                //nrch = '7',
                //data = data_dp,
                //nrcarnet = dpl.nrcarnet,                    
                //lunaajdeces = pens_up_dp.lunaajdeces,                    
                //dobanda_penalizatoare = dpl.dp,
                //dobanda = 0,                    
                //total = dpl.dp,
                //nume = pens_up_dp.nume,
                //tip_operatie = "NC",
                //tip_document = 11
                //});
                //_context.SaveChanges();

                //chidmax_ins_cr_rstnt = _context.Chitantas.Max(c => c.idchitanta);
                chidmax_ins_cr_rstnt = 0;
                //de aici dec 2023

                

                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = data_dp,
                    id_document = chidmax_ins_cr_rstnt,
                    nr_document = "6",
                    id_CS_debitor = 109,
                    id_CA_debitor = Int32.Parse("6791" + pinsD4611.nrcarnet.ToString().Remove(0, 4)),
                    id_CS_creditor = 53,
                    id_CA_creditor = 103,
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = 0,
                    debit = dobandaIns,
                    credit = dobandaIns,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = 0,
                    tip_document = 11,
                    sortare = "H"
                });

                pinsD4611.solddobanda = pinsD4611.solddobanda + dobandaIns;
                _context.SaveChanges();

                _context.registru_jurnal.Add(new registru_jurnal
                {
                    data = data_dp,
                    id_document = chidmax_ins_cr_rstnt,
                    nr_document = "7",
                    id_CS_debitor = 108,
                    id_CA_debitor = Int32.Parse("6792" + pinsD4611.nrcarnet.ToString().Remove(0, 4)),
                    id_CS_creditor = 53,
                    id_CA_creditor = 103,
                    explicatie_nr_cont_analitic = "-",
                    SI_CS_debit = 0,
                    SI_CA_debit = 0,
                    SI_CS_credit = 0,
                    SI_CA_credit = 0,
                    debit = dpIns,
                    credit = dpIns,
                    SF_CS_debit = 0,
                    SF_CA_debit = 0,
                    SF_CS_credit = 0,
                    SF_CA_credit = 0,
                    tip_document = 11,
                    sortare = "H"
                });

                pinsD4611.sold_DP = pinsD4611.sold_DP + dpIns;
                _context.SaveChanges();



            }

            return View();
        }

        public ActionResult restantieri_contributii(int? page = 1)
        {
            DateTime now = DateTime.Now;

            var restante_contributii_nrluni = _context.Pensionars
                              .Where(i => (i.id_stare == 0) && ((now.Year * 12 + now.Month) - (i.lunaajdeces.Year * 12 + i.lunaajdeces.Month)) > 6).OrderBy(i => i.lunaajdeces);
                       

            var pageNumber = page ?? 1;
            ViewData["restantieri_contributii"] = restante_contributii_nrluni.ToList().ToPagedList(pageNumber, 20);

            return View(ViewData["restantieri_contributii"]);            

        }


        public ActionResult listare_restantieri_contributii(int? page = 1)
        {
            DateTime nowl = DateTime.Now;

            var restante_contributii_nrluni_l = _context.Pensionars
                             .Where(i => (i.id_stare == 0) && ((nowl.Year * 12 + nowl.Month) - (i.lunaajdeces.Year * 12 + i.lunaajdeces.Month)) > 6).OrderBy(i => i.lunaajdeces);


            MemoryStream ms = new MemoryStream();
            PdfWriter writer = new PdfWriter(ms);
            PdfDocument pdfDoc = new PdfDocument(writer);
            Document document = new Document(pdfDoc, iText.Kernel.Geom.PageSize.A4, false);
            writer.SetCloseStream(false);

            document.SetMargins(20f, 20f, 20f, 40f);

            Style normalCL = new Style();
            PdfFont font = PdfFontFactory.CreateFont(StandardFonts.HELVETICA);
            normalCL.SetFont(font).SetFontSize(12);

            if (restante_contributii_nrluni_l.Count() > 0)
            {
                foreach (Pensionar pensionarsel in restante_contributii_nrluni_l)
                {
                    Paragraph p1CL = new Paragraph("C.A.R. Pensionari Turnu Magurele")
                        .SetTextAlignment(TextAlignment.LEFT)
                        .SetBold().AddStyle(normalCL);
                    document.Add(p1CL);

                    Paragraph p2CL = new Paragraph("Nr .............. din ..............")
                        .SetTextAlignment(TextAlignment.LEFT)
                        .AddStyle(normalCL);
                    document.Add(p2CL);

                    Paragraph spatiuCL = new Paragraph("");
                    document.Add(spatiuCL);
                    document.Add(spatiuCL);

                    Paragraph p3CL = new Paragraph("CATRE DEBITORUL")
                       .SetTextAlignment(TextAlignment.LEFT)
                       .SetBold().AddStyle(normalCL);
                    document.Add(p3CL);

                    document.Add(spatiuCL);

                    Paragraph pnumenrc = new Paragraph("Nume: " + pensionarsel.nume.Trim() + ", " + "nr carnet: " + pensionarsel.nrcarnet.ToString()) 
                       .SetTextAlignment(TextAlignment.LEFT)
                       .AddStyle(normalCL);
                    document.Add(pnumenrc);

                    Paragraph pJL = new Paragraph("judet: " + pensionarsel.judet.Trim() + ", " + "localitate: " + pensionarsel.localitate.Trim() )
                        .SetTextAlignment(TextAlignment.LEFT)
                      .AddStyle(normalCL);
                    document.Add(pJL);


                    Paragraph padr = new Paragraph("strada: " + pensionarsel.strada.Trim() + ", " + "nr: " + pensionarsel.nr.Trim() + ", " + "localitate: " + pensionarsel.bloc.Trim() + ", " + "localitate: " + pensionarsel.ap.Trim())
                       .SetTextAlignment(TextAlignment.LEFT)
                      .AddStyle(normalCL);
                    document.Add(padr);

                    document.Add(spatiuCL);

                    Paragraph pnotif = new Paragraph("Va informam prin prezenta ca, in termen de zece zile de la primirea prezentei adrese, sa va prezentati la casieria unitatii pentru plata cotizatiilor restante. In caz contrar veti fi exclusi din randurile membrilor")
                       .SetTextAlignment(TextAlignment.LEFT)
                      .AddStyle(normalCL);
                    document.Add(pnotif);

                    document.Add(spatiuCL);
                    document.Add(spatiuCL);

                    Paragraph pSIGN = new Paragraph("PRESEDINTE                                                                                       CONTABIL SEF")
                       .SetTextAlignment(TextAlignment.LEFT).SetBold()
                      .AddStyle(normalCL);
                    document.Add(pSIGN);
                    document.Add(spatiuCL);
                    document.Add(spatiuCL);
                    document.Add(spatiuCL);
                    document.Add(spatiuCL);
                    document.Add(spatiuCL);
                    document.Add(spatiuCL);
                    document.Add(spatiuCL);
                    document.Add(spatiuCL);
                    document.Add(spatiuCL);
                    document.Add(spatiuCL);
                    document.Add(spatiuCL);
                    document.Add(spatiuCL);
                    document.Add(spatiuCL);

                    //   pdfincasari += "<p><b>C.A.R. Pensionari Turnu Magurele <br> " + " Nr .............. din .............. </b> <br> <br>  <b>    NOTIFICARE </b> <br> <br> </p>" +
                    //     "<p><b> CATRE DEBITORUL </b> <br> <br>  Nume: " + pensionarsel.nume + ", &nbsp;  nr carnet " + pensionarsel.nrcarnet + ", &nbsp; judetul " + pensionarsel.judet + ",&nbsp;" +
                    //              ", &nbsp;  localitate " + pensionarsel.localitate + ", &nbsp;  strada " + pensionarsel.strada + ", &nbsp;  nr " + pensionarsel.nr + ", &nbsp;  ap " + pensionarsel.ap +
                    //            "<br> <br> <br>  </p>" +

                    //          "<p> Va informam prin prezenta ca, in termen de zece zile de la primirea prezentei adrese, sa va prezentati la casieria unitatii pentru plata cotizatiilor restante. In caz contrar veti fi exclusi din randurile membrilor </p>" +

                    //        "<br> <br> " + "<p>PRESEDINTE &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; &nbsp; CONTABIL SEF <br> <br> <br> <br> <br> <br> <br> <br> </p>";

                }
            }



            document.Close();
            byte[] byteInfo = ms.ToArray();
            ms.Write(byteInfo, 0, byteInfo.Length);
            ms.Position = 0;

            FileStreamResult fileStreamResult = new FileStreamResult(ms, "application/pdf");
            // using (FileStream fs = File.Create("C:\\test.pdf")) { fs.Write(byteInfo, 0, (int)byteInfo.Length); } 

            //File(byteInfo, "application/pdf", "E:\\JOB\\test.pdf");
            return fileStreamResult;

            
        }

        public ActionResult check_conturi(int id_cs_in = 0)
        {
            id_cs_in = 26791;


            int luna = 0;
            luna = 9;

            long id_ca_26791 = 0;
            decimal credit_26791 = 0;
            decimal debit_26791 = 0;
            decimal total_26791 = 0;
            decimal dobnupay = 0;
            string erori = " ";

            decimal diferenta = 0;
            decimal si = 0;
            decimal sf = 0;
            long id_cs = 0, id_ca = 0;
            decimal credit, debit;
            string nrcontCS = "";

            if (id_cs_in == 4622) id_cs = 106;
            if (id_cs_in == 113) id_cs = 101;
            if (id_cs_in == 2678) id_cs = 103;
            if (id_cs_in == 4614) id_cs = 26;
            if (id_cs_in == 4611) id_cs = 27;

            if (id_cs_in == 26792) id_cs = 108;
            if (id_cs_in == 26791) id_cs = 109;


            if (id_cs_in == 4622) nrcontCS = "4622";
            if (id_cs_in == 113) nrcontCS = "113";
            if (id_cs_in == 2678) nrcontCS = "2678";
            if (id_cs_in == 4614) nrcontCS = "4614";
            if (id_cs_in == 26792) nrcontCS = "6792";
            if (id_cs_in == 26791) nrcontCS = "6791";

            var pens_cek_26791 = _context.Pensionars.OrderBy(pcd => pcd.nrcarnet).ToList();

            var rj_cek_cont = _context.registru_jurnal.Where(rjc26791 => (rjc26791.data.Month == 1) && (rjc26791.id_CA_creditor == 67923893));

            if (id_cs_in == 2678)
                pens_cek_26791 = _context.Pensionars.Where(pcd => pcd.soldimp > 0).OrderBy(pcd => pcd.nrcarnet).ToList();

          //  if (id_cs_in == 26791)
              //  pens_cek_26791 = _context.Pensionars.Where(pcd => pcd.soldimp > 0).OrderBy(pcd => pcd.nrcarnet).ToList();

            if (id_cs_in == 4614)
                pens_cek_26791 = _context.Pensionars.Where(pcd => pcd.id_stare == 3).OrderBy(pcd => pcd.nrcarnet).ToList();
            if (id_cs_in == 4611)
                pens_cek_26791 = _context.Pensionars.Where(pcd => pcd.id_stare == 4).OrderBy(pcd => pcd.nrcarnet).ToList();

            foreach (Pensionar pcd in pens_cek_26791)
            {
                //imitializari
                id_ca = Int64.Parse(nrcontCS + pcd.nrcarnet);
                if (id_cs_in == 4614) id_ca = pcd.nrcarnet;
                if (id_cs_in == 4611) id_ca = pcd.nrcarnet;

                credit = 0;
                rj_cek_cont = _context.registru_jurnal.Where(rjc26791 => (rjc26791.id_CS_creditor == id_cs) && (rjc26791.id_CA_creditor == id_ca) && (rjc26791.data.Year==DateTime.Now.Year));
                if (rj_cek_cont.Count() != 0) credit = rj_cek_cont.Sum(rjc26791 => rjc26791.credit);

                debit = 0;
                rj_cek_cont = _context.registru_jurnal.Where(rjc26791 => (rjc26791.id_CS_debitor == id_cs) && (rjc26791.id_CA_debitor == id_ca) && (rjc26791.data.Year == DateTime.Now.Year));
                if (rj_cek_cont.Count() != 0) debit = rj_cek_cont.Sum(rjc26791 => rjc26791.credit);

                sf = 0;
                si = 0;

                if (id_cs_in == 113)
                {
                    sf = pcd.soldcotiz;
                    si = pcd.soldcotiz2016;
                }

                if (id_cs_in == 4622)
                {
                    sf = pcd.credit_imprumut;
                    si = pcd.credit_imprumut2016;
                }

                if (id_cs_in == 2678)
                {
                    sf = pcd.soldimp - pcd.debit_imprumut;
                    si = pcd.soldimp2016 - pcd.debit_imprumut2016;
                }


                if (id_cs_in == 4614)
                {
                    sf = pcd.sold_461;
                    si=pcd.sold_461_2016;
                }


                if (id_cs_in == 26792)
                {
                    si = pcd.sold_DP2016;
                }

                if (id_cs_in == 26792)
                {
                    sf = pcd.sold_DP;
                }



                if (id_cs_in == 26791)
                {
                    si = pcd.sold_dobanda2016;
                }

                if (id_cs_in == 26791)
                {
                    sf = pcd.solddobanda;
                }


                diferenta = 0;

                diferenta = sf - (si - debit + credit);

                if (id_cs_in == 2678) diferenta = sf - (si + debit - credit);
                if (id_cs_in == 4614) diferenta = sf - (si + debit - credit);
                if (id_cs_in == 26792) diferenta = sf - (si + debit - credit);
                if (id_cs_in == 26791) diferenta = sf - (si + debit - credit);

                _context.SaveChanges();

                if (diferenta != 0) erori = erori + ", nrcarnet=" + pcd.nrcarnet + ", " + diferenta + " " + "</n>";

            }

            ViewBag.solduri_gresite = erori;

            return View();


        }


        public ActionResult get_dob_nupay(int id_cs_in = 0)
        {
            //select * from dobanda neplatita where luna=12
            //if exista ch cu rata remove / dob =0
            int luna = DateTime.Now.Month;

            var ch_GDNP = _context.Chitantas
           .Where(cgdnp => (cgdnp.nrcarnet == 3893) && (cgdnp.data.Month == luna) && (cgdnp.dobanda != 0)).SingleOrDefault();
            var desf_GDNP_month = _context.desfasurator_rate
                  .Where(dgdnp => (dgdnp.nrcarnet == 3893) && (dgdnp.data_rata.Month == luna) && (dgdnp.data_rata.Year == DateTime.Now.Year)).SingleOrDefault();
            var pensGetDobNuPay = _context.Pensionars.OrderBy(pgdnp => pgdnp.nrcarnet).ToList();
            var dob_neplatita = _context.dobanda_neplatita.Where(dp => dp.nrcarnet == 1173).SingleOrDefault();
            var dob_nupay_cekpay = _context.dobanda_neplatita.OrderBy(dncp => dncp.nrcarnet);

            decimal dobNuPay = 0;

            foreach (dobanda_neplatita dp in dob_nupay_cekpay)
            {
            ch_GDNP = _context.Chitantas
            .Where(cgdnp => (cgdnp.nrcarnet == dp.nrcarnet) && (cgdnp.data.Month == luna - 1) && (cgdnp.dobanda != 0)).SingleOrDefault();
                if (ch_GDNP != null)
                {
                    dp.dobanda = 0;
                    _context.SaveChanges();
                }
            }


            foreach (Pensionar pgdnp in pensGetDobNuPay)
            {
            
            dobNuPay=0;
            desf_GDNP_month = _context.desfasurator_rate.Where(dgdnp => (dgdnp.data_rata.Month == luna-1) && (dgdnp.data_rata.Year == DateTime.Now.Year) && (dgdnp.nrcarnet == pgdnp.nrcarnet)).SingleOrDefault();
            if (desf_GDNP_month != null)
            {
                ch_GDNP = _context.Chitantas
                   .Where(cgdnp => (cgdnp.nrcarnet == pgdnp.nrcarnet) && (cgdnp.data.Month == luna - 1) && (cgdnp.dobanda != 0)).SingleOrDefault();

                if (ch_GDNP == null)
                {

                dobNuPay = desf_GDNP_month.dobanda ;               

                dob_neplatita = _context.dobanda_neplatita.Where(dp => dp.nrcarnet == pgdnp.nrcarnet).SingleOrDefault();
                if (dob_neplatita != null) dob_neplatita.dobanda = dob_neplatita.dobanda + dobNuPay;
                if (dob_neplatita == null)
                    _context.dobanda_neplatita.Add(new dobanda_neplatita
                    {
                        nrcarnet = pgdnp.nrcarnet,
                        dobanda = dobNuPay,
                        luna = 2025,

                    });
                _context.SaveChanges();
                        
                }
            }

            
            

            }

            decimal TotalDobNupay = 0;
            TotalDobNupay = _context.dobanda_neplatita.Sum(tdnp => tdnp.dobanda);

            ViewData["totalDobNupay"] = "Total dobanda neplatita luna " +  DateTime.Now.Month + " "  +  TotalDobNupay.ToString("N2");

            return View();
        }


        public ActionResult check_balanta_26791()
        {
            //init
            //data_ref


            int luna = 0;
            luna = DateTime.Now.Month-1;
            if (DateTime.Now.Month == 12) luna = 12;
            // DateTime data_ref = Convert.ToDateTime("2025-4-1 00:00:00");
            DateTime data_ref;
            string data_refstr = "";
            data_refstr = DateTime.Now.Year.ToString() + "-" + DateTime.Now.Month.ToString() + "-1 00:00:00";
            if (DateTime.Now.Month == 12) data_refstr = DateTime.Now.Year.ToString() + "-1-1 00:00:00";
            data_ref = Convert.ToDateTime(data_refstr);


            long id_ca_26791 = 0;
            decimal credit_26791 = 0;
            decimal debit_26791 = 0;
            decimal total_26791 = 0;
            decimal dobnupay = 0;
            string erori = " ";

            var pens_cek_26791 = _context.erori_103_109.OrderBy(pcd => pcd.nrcarnet).ToList();
            var rj_cek_26791 = _context.registru_jurnal.Where(rjc26791 => (rjc26791.data.Month == 1) && (rjc26791.id_CA_creditor == 67923893));
            var desf_cek_26791 = _context.desfasurator_rate_luna.Where(dc26791 => (dc26791.data_rata > data_ref) && (dc26791.nrcarnet == 3893));
            var get_Dob_NuPay =_context.dobanda_neplatita.Where(gdnp => (gdnp.nrcarnet == 81) ).SingleOrDefault();

            foreach (erori_103_109   pcd in pens_cek_26791)
            {
                credit_26791 = 0;
                id_ca_26791 = Int32.Parse("6791" + pcd.nrcarnet);
                rj_cek_26791 = _context.registru_jurnal.Where(rjc26791 => (rjc26791.data.Month == luna) && (rjc26791.id_CA_creditor == id_ca_26791));
                if (rj_cek_26791.Count() != 0) credit_26791 = rj_cek_26791.Sum(rjc26791 => rjc26791.credit);

                debit_26791 = 0;
                rj_cek_26791 = _context.registru_jurnal.Where(rjc26791 => (rjc26791.data.Month == luna) && (rjc26791.id_CA_debitor == id_ca_26791));
                if (rj_cek_26791.Count() != 0) debit_26791 = rj_cek_26791.Sum(rjc26791 => rjc26791.debit);


                total_26791 = 0;
                desf_cek_26791 = _context.desfasurator_rate_luna.Where(dc26791 => (dc26791.data_rata > data_ref) && (dc26791.nrcarnet == pcd.nrcarnet));
                if (desf_cek_26791.Count() != 0) total_26791 = desf_cek_26791.Sum(dc26791 => dc26791.dobanda);

                dobnupay = 0;
                get_Dob_NuPay = _context.dobanda_neplatita.Where(gdnp => (gdnp.nrcarnet == pcd.nrcarnet)).SingleOrDefault();
                if (get_Dob_NuPay != null) dobnupay = get_Dob_NuPay.dobanda;

                pcd.credit_109 = credit_26791;
                pcd.debit_109 = debit_26791;
                pcd.SF_109 = total_26791;
                pcd.dobanda_neplatita = dobnupay;

                pcd.erori_109 = total_26791 + dobnupay - (pcd.SI_109 + debit_26791 - credit_26791);

                _context.SaveChanges();

                if (pcd.erori_109 != 0) erori = erori + ", nrcarnet=" + pcd.nrcarnet + ", " + pcd.erori_109 + " ";

            }

            ViewBag.solduri_gresite = erori;

            return View();


        }

        public ActionResult cek_balanta_2678()
        {
            //MAI TREBUIE 
            //DATA REF

            int id_ca_2678 = 0;
            int id_ca_4622 = 0;
            int id_ca_4612 = 0;
            decimal total_incasari_casa_2678 = 0;
            decimal total_incasari_banca_2678 = 0;
            decimal total_iesiri_2678 = 0;
            decimal total_intrari_2678 = 0;
            decimal total_plati_casa_2678 = 0;
            decimal total_plati_banca_2678 = 0;
            decimal total_RP = 0;
            int luna = 0;

            //INITIALIZARE
            luna = DateTime.Now.Month-1;

            //mai  am de facut
            DateTime data_ref = Convert.ToDateTime("2025-6-1 00:00:00");              
            //end de facut

            var Incasari2678 = _context.registru_jurnal.Where(inc2678 => (inc2678.id_CS_debitor == 1) && (inc2678.id_CS_creditor == 1));
            var Iesiri2678 = _context.registru_jurnal.Where(out2678 => (out2678.id_CS_debitor == 1) && (out2678.id_CS_creditor == 1));
            var Intrari2678 = _context.registru_jurnal.Where(out2678 => (out2678.id_CS_debitor == 1) && (out2678.id_CS_creditor == 1));
            var Plati2678 = _context.registru_jurnal.Where(out2678 => (out2678.id_CS_debitor == 1) && (out2678.id_CS_creditor == 1));
            var get_total_RP =_context.desfasurator_rate_luna.Where(gtrp => (gtrp.data_rata > data_ref) && (gtrp.nrcarnet == 3893));
            //end init

            var pensCekBalanta2678 = _context.erori_103_109.OrderBy(pcd => pcd.nrcarnet).ToList();

            foreach (erori_103_109 pcb2678 in pensCekBalanta2678)
            {
                //CREDIT
                //CREDIT incasari 5131 2678
                id_ca_2678 = Int32.Parse("2678" + pcb2678.nrcarnet);
                total_incasari_casa_2678 = 0;
                Incasari2678 = _context.registru_jurnal.Where(inc2678 => (inc2678.id_CS_debitor == 51) && (inc2678.id_CA_creditor == id_ca_2678) && (inc2678.data.Month == luna));
                if (Incasari2678.Count() != 0) total_incasari_casa_2678 = Incasari2678.Sum(inc2678 => inc2678.credit);
                //CREDIT incasari 5121 2678              
                total_incasari_banca_2678 = 0;
                Incasari2678 = _context.registru_jurnal.Where(inc2678 => (inc2678.id_CS_debitor == 52) && (inc2678.id_CA_creditor == id_ca_2678) && (inc2678.data.Month == luna));
                if (Incasari2678.Count() != 0) total_incasari_banca_2678 = Incasari2678.Sum(inc2678 => inc2678.credit);
                //CREDIT iesiri 2678
                id_ca_4622 = Int32.Parse("4622" + pcb2678.nrcarnet);
                total_iesiri_2678 = 0;
                Iesiri2678 = _context.registru_jurnal.Where(out2678 => (out2678.id_CA_debitor == id_ca_4622) && (out2678.id_CA_creditor == id_ca_2678) && (out2678.data.Month == luna));
                if (Iesiri2678.Count() != 0) total_iesiri_2678 = Iesiri2678.Sum(out2678 => out2678.credit);
                //CREDIT intrari 2678
                id_ca_4612 = Int32.Parse("4612" + pcb2678.nrcarnet);
                total_intrari_2678 = 0;
                Intrari2678 = _context.registru_jurnal.Where(in2678 => (in2678.id_CA_debitor == id_ca_4612) && (in2678.id_CA_creditor == id_ca_2678) && (in2678.data.Month == luna));
                if (Intrari2678.Count() != 0) total_intrari_2678 = Intrari2678.Sum(in2678 => in2678.credit);

                //DEBIT
                //PLATI PRIN CASA
                total_plati_casa_2678 = 0;
                Plati2678 = _context.registru_jurnal.Where(pay2678 => (pay2678.id_CA_debitor == id_ca_2678) && (pay2678.id_CS_creditor == 51) && (pay2678.data.Month == luna));
                if (Plati2678.Count() != 0) total_plati_casa_2678 = Plati2678.Sum(pay2678 => pay2678.debit);
                //PLATI PRIN banca
                total_plati_banca_2678 = 0;
                Plati2678 = _context.registru_jurnal.Where(pay2678 => (pay2678.id_CA_debitor == id_ca_2678) && (pay2678.id_CS_creditor == 52) && (pay2678.data.Month == luna));
                if (Plati2678.Count() != 0) total_plati_banca_2678 = Plati2678.Sum(pay2678 => pay2678.debit);

                //total RP
                total_RP = 0;
                get_total_RP = _context.desfasurator_rate_luna.Where(gtrp => (gtrp.nrcarnet == pcb2678.nrcarnet) && (gtrp.data_rata > data_ref));
                if (get_total_RP.Count() != 0) total_RP = get_total_RP.Sum(gtrp => gtrp.rata_de_plata);

                pcb2678.credit_103 = total_incasari_casa_2678+ total_incasari_banca_2678;
                pcb2678.debit_103 = total_plati_casa_2678 + total_plati_banca_2678;
                pcb2678.SF_103 = total_RP;            
                pcb2678.erori_103 = total_RP  - (pcb2678.SI_103 + pcb2678.debit_103  - pcb2678.credit_103 -total_iesiri_2678 - total_intrari_2678 );

                
                _context.SaveChanges();


            }
            //pa cek


            return View();
        }

        public decimal round_value(decimal dbnda)
        {
            //impr 2025
            //Math.Round(3.32, 1, MidpointRounding.AwayFromZero) 
            decimal rest = dbnda - Math.Truncate(dbnda);

            int rest1 = 99;
            int rest2 = 88;

            if (rest.ToString().Length > 4)
            {
                rest1 = Convert.ToInt32(rest.ToString().Substring(2, 2));
                rest2 = Convert.ToInt32(rest.ToString().Substring(4, 1));

                if (rest2 >= 5) rest1 = rest1 + 1;

                if (rest1 != 100) dbnda = Convert.ToDecimal(Math.Truncate(dbnda).ToString() + "." + rest1);
                if (rest1 == 100) dbnda = Convert.ToDecimal((Math.Truncate(dbnda) + 1).ToString() + ".00");


                if (Convert.ToInt32(rest.ToString().Substring(2, 1)) == 0) dbnda = Convert.ToDecimal(Math.Truncate(dbnda).ToString() + ".0" + rest1);
            }

            return dbnda;

        }



        public async Task<ActionResult> cek_dp()
        {
            var pensdp = _context.Pensionars.Where(pdp => (pdp.sold_DP > 0) && (pdp.nrcarnet < 20000)).OrderBy(pdp => pdp.nrcarnet);
            var rjdp = _context.registru_jurnal.Where(rjdp => (rjdp.id_CA_debitor == 1) && (rjdp.data > DateTime.Now));

            int idcadp = 0;
            decimal sumdp = 0;

            foreach (Pensionar pdp in pensdp)
            {
                idcadp = Int32.Parse("6792" + pdp.nrcarnet.ToString());
                var chdp = _context.Chitantas.Where(cdp => (cdp.nrcarnet == pdp.nrcarnet) && (cdp.dobanda_penalizatoare != 0)).OrderByDescending(cdp => cdp.data);
                sumdp = 0;
                if (chdp.Count() > 0)
                {
                    rjdp = _context.registru_jurnal.Where(rjdp => (rjdp.id_CA_debitor == idcadp) && (rjdp.data > chdp.First().data));
                    if (rjdp.Count() > 0) sumdp = rjdp.Sum(rjdp => rjdp.credit);
                }


                if (pdp.sold_DP != sumdp)
                {
                    pdp.soldtaxainscr = sumdp;
                    _context.SaveChanges();
                }


            }

            return View();
        }


        [Authorize(Roles = "CARP_TM\\CARP_TM_ADMIN")]
        public ActionResult administrare()
        {
           
            return View();
        }



        private bool PensionarExists(int id)
        {
            return _context.Pensionars.Any(e => e.nrcarnet == id);
        }
    }
}

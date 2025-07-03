using carp_tm_2025.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using carp_tm_2025.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace carp_tm_delegati.Controllers
{
    [Authorize(Roles = "CARP_TM\\CARP_TM_READ")]
    public class girantisController : Controller
    {
        private readonly carp_tm_2025Context _context;

        public girantisController(carp_tm_2025Context context)
        {
            _context = context;
        }
        public IActionResult Index(int nrcarnetg = 0, string cnp = "9999")
        {
            var giranti = _context.girantis.Where(g => g.CNP.ToString().Contains(cnp.Trim())).ToList();

            ViewData["nrcarnetg"] = nrcarnetg.ToString();
            return View(giranti);
        }


        // GET: Pensionars/Create
        public IActionResult Create( int nrcg=0)
        {
            ViewData["nrcarnetgc"] = nrcg;

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("nrcarnet,nume_prenume,CNP,judet,localitate,strada,nr,bloc,ap,venit,telefon")] giranti girantis)
        {       


            
            var imprg = await _context.imprumuturis.Where(ig => ig.nrcarnet == girantis.nrcarnet).OrderByDescending(ig=>ig.data_imprumut).ToListAsync();

            //ViewData["dataimpg"] = imprg.First().data_imprumut;
            //ViewData["valimpg"] = imprg.acimp;
            girantis.data_imprumut = imprg.First().data_imprumut;
            girantis.valoare_imprumut = imprg.First().valoare_imprumut;
            girantis.nr_contract = imprg.First().nr_contract;
            //ViewData["valimpg"] = imprg.acimp;
            //if giranti se baga before bag impr
            // if var impr de sus is  null idimpr=0 si bag de mana data si valimpr


            if (ModelState.IsValid)
            {             


                _context.girantis.Add(girantis);
                await _context.SaveChangesAsync();

               // return View();
                return RedirectToAction("Index", "imprumuturis",  new { searchterm = girantis.nrcarnet });
            }

            return View();
        }


        public async Task<IActionResult> giranti_imprumut(int nrcarnetg,  int nrcontractg = 0)
        {
           var getgir = await _context.girantis.Where(gi => (gi.nr_contract == nrcontractg) && (gi.nrcarnet == nrcarnetg) ).OrderBy(gi=>gi.nume_prenume).ToListAsync();
            ViewData["nrcarnetg"] = nrcarnetg.ToString();
            ViewData["nrcontractg"] = nrcontractg.ToString();

            return View(getgir);
        }


        public async Task<IActionResult> cins()
        {
            var goins = await _context.go.OrderBy(gi => gi.id_girant).ToListAsync();
            var imprgo = await _context.imprumuturis.Where(igo => igo.nrcarnet ==0).ToListAsync();

            foreach (go ggoo in goins)
            {
            imprgo = await _context.imprumuturis.Where(gi => (gi.nr_contract == ggoo.nr_contract) && (gi.data_imprumut.Year==ggoo.an)).ToListAsync();
            if ((imprgo.Count() == 0) || (imprgo.Count() > 1)) ggoo.nume_prenume = ggoo.nume_prenume + "X";

                    if (imprgo.Count() == 1)
                    _context.girantis.Add(new giranti
                    {
                       nrcarnet=imprgo.First().nrcarnet,
                        nume_prenume=ggoo.nume_prenume,
                        CNP=ggoo.CNP,
                        judet= "-",
                        localitate = "-",
                        strada = "-",
                        nr = 0,
                        bloc = "-",
                        ap = "-",
                        venit=0,
                        telefon = 0,
                        nr_contract = ggoo.nr_contract,
                        data_imprumut= imprgo.First().data_imprumut,
                        valoare_imprumut = imprgo.First().valoare_imprumut
                    });

                await _context.SaveChangesAsync();
                   
        }

           

            return View();
        }

    }
}
